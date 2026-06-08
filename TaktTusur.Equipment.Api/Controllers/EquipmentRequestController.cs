using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaktTusur.Equipment.Api.Contracts.EquipmentRequests;
using TaktTusur.Equipment.Api.Services.Users;
using TaktTusur.Equipment.DataAccess;
using TaktTusur.Equipment.Domain.EquipmentRequest;
using TaktTusur.Equipment.Domain.Users;

namespace TaktTusur.Equipment.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EquipmentRequestController : ControllerBase
{
    private readonly EquipmentDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly IUserRolesService _userRolesService;

    public EquipmentRequestController(
        EquipmentDbContext dbContext,
        IUserService userService,
        IUserRolesService userRolesService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _userRolesService = userRolesService;
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentRequestResponseDto>> CreateEquipmentRequest(
        [FromBody] CreateEquipmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        var canRequestEquipment = await _userRolesService.CanRequestEquipmentAsync(currentUser.Id);

        if (!canRequestEquipment)
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;
        var equipmentRequest = new EquipmentRequest
        {
            Id = Guid.NewGuid(),
            RequestText = request.RequestText.Trim(),
            DateOfIssue = request.DateOfIssue,
            DateOfReturn = request.DateOfReturn,
            CreatedAt = now,
            UpdatedAt = now,
            Status = EquipmentRequestStatus.New,
            UserId = currentUser.Id
        };

        _dbContext.EquipmentRequests.Add(equipmentRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Created($"/api/v1/EquipmentRequest/{equipmentRequest.Id}", ToResponseDto(equipmentRequest));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipmentRequestResponseDto>> EditEquipmentRequest(
        Guid id,
        [FromBody] UpdateEquipmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        var canRequestEquipment = await _userRolesService.CanRequestEquipmentAsync(currentUser.Id);

        if (!canRequestEquipment)
        {
            return Forbid();
        }

        var equipmentRequest = await _dbContext.EquipmentRequests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipmentRequest is null)
        {
            return NotFound();
        }

        if (equipmentRequest.UserId != currentUser.Id)
        {
            return Forbid();
        }

        if (equipmentRequest.Status is EquipmentRequestStatus.Approved or EquipmentRequestStatus.Rejected)
        {
            return BadRequest("Approved or rejected equipment requests cannot be edited.");
        }

        equipmentRequest.RequestText = request.RequestText.Trim();
        equipmentRequest.DateOfIssue = request.DateOfIssue;
        equipmentRequest.DateOfReturn = request.DateOfReturn;
        equipmentRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponseDto(equipmentRequest));
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<EquipmentRequestResponseDto>>> GetMyRequests(CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        var requests = await _dbContext.EquipmentRequests
            .Where(x => x.UserId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);

        return Ok(requests);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<EquipmentRequestResponseDto>>> GetPendingRequests(CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!await CanIssueEquipmentAsync(currentUser))
        {
            return Forbid();
        }

        var requests = await _dbContext.EquipmentRequests
            .Where(x => x.Status == EquipmentRequestStatus.New)
            .OrderBy(x => x.CreatedAt)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);

        return Ok(requests);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<EquipmentRequestResponseDto>> ApproveRequest(
        Guid id,
        [FromBody] ApproveEquipmentRequestDto? request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!await CanIssueEquipmentAsync(currentUser))
        {
            return Forbid();
        }

        var equipmentRequest = await FindEquipmentRequestAsync(id, cancellationToken);
        if (equipmentRequest is null)
        {
            return NotFound();
        }

        if (equipmentRequest.Status != EquipmentRequestStatus.New)
        {
            return BadRequest("Only new equipment requests can be approved.");
        }

        if (!string.IsNullOrWhiteSpace(request?.RequestText))
        {
            equipmentRequest.RequestText = request.RequestText.Trim();
        }

        equipmentRequest.Status = EquipmentRequestStatus.Approved;
        equipmentRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponseDto(equipmentRequest));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<EquipmentRequestResponseDto>> RejectRequest(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!await CanIssueEquipmentAsync(currentUser))
        {
            return Forbid();
        }

        var equipmentRequest = await FindEquipmentRequestAsync(id, cancellationToken);
        if (equipmentRequest is null)
        {
            return NotFound();
        }

        if (equipmentRequest.Status != EquipmentRequestStatus.New)
        {
            return BadRequest("Only new equipment requests can be rejected.");
        }

        equipmentRequest.Status = EquipmentRequestStatus.Rejected;
        equipmentRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponseDto(equipmentRequest));
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<ActionResult<EquipmentRequestResponseDto>> IssueEquipment(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!await CanIssueEquipmentAsync(currentUser))
        {
            return Forbid();
        }

        var equipmentRequest = await FindEquipmentRequestAsync(id, cancellationToken);
        if (equipmentRequest is null)
        {
            return NotFound();
        }

        if (equipmentRequest.Status is not (EquipmentRequestStatus.New or EquipmentRequestStatus.Approved))
        {
            return BadRequest("Only new or approved equipment requests can be issued.");
        }

        equipmentRequest.Status = EquipmentRequestStatus.Issued;
        equipmentRequest.IssuedByUserId = currentUser.Id;
        equipmentRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponseDto(equipmentRequest));
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<EquipmentRequestResponseDto>> ReturnEquipment(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!await CanIssueEquipmentAsync(currentUser))
        {
            return Forbid();
        }

        var equipmentRequest = await FindEquipmentRequestAsync(id, cancellationToken);
        if (equipmentRequest is null)
        {
            return NotFound();
        }

        if (equipmentRequest.Status != EquipmentRequestStatus.Issued)
        {
            return BadRequest("Only issued equipment requests can be returned.");
        }

        equipmentRequest.Status = EquipmentRequestStatus.Returned;
        equipmentRequest.ClosedByUserId = currentUser.Id;
        equipmentRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponseDto(equipmentRequest));
    }

    private Task<EquipmentRequest?> FindEquipmentRequestAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.EquipmentRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private Task<bool> CanIssueEquipmentAsync(User user)
    {
        return _userRolesService.CanIssueEquipmentAsync(user.Id);
    }

    private static EquipmentRequestResponseDto ToResponseDto(EquipmentRequest request)
    {
        return new EquipmentRequestResponseDto
        {
            Id = request.Id,
            RequestText = request.RequestText,
            DateOfIssue = request.DateOfIssue,
            DateOfReturn = request.DateOfReturn,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            Status = request.Status,
            UserId = request.UserId,
            IssuedByUserId = request.IssuedByUserId,
            ClosedByUserId = request.ClosedByUserId
        };
    }
}
