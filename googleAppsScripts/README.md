# Script for Google Apps Script
It shows the equipment information, which is taken from Google Spreadsheets(special table for club storage)

## Table
![image](https://github.com/takttusur/takt-equipment/assets/15832039/6f835932-986f-4ae2-a875-d5e3f33b6d91)

## Deploy

### First time
1. Open the table
2. In top-menu click Extensions -> Apps Script
3. Put the code from `storageTableApi.gs` to editor and click save
4. Click on project name `Untitled Project` and change it to `Equipment`
5. Click `+` near Services on the left panel
6. Choose `Google Sheets API`, version `v4`, identifier `Sheets`
7. Click `Add`
8. Click `Deploy -> New deployment` on top right
9. Choose Select type `Web App`,  Execute as `Me(your@email)` and Who has access `Anyone`
10. Copy `Web app - URL` and add it to Background worker of Equipment Service.

### Update

1. Open the table
2. Go to Extensions -> Apps Script
3. Find Code.gs file and open it
4. Remove all code from Code.gs and put a code from storageTableApi.gs
5. Save it
6. Click `Deploy -> Manage deployments`
7. Choose active deployment and click EFdit(pencil icon)
8. For `Version` choose `New version`
9. Click `Deploy`
