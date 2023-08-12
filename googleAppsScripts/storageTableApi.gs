const SECRET = "1";
const STORE_SHEET_NAME="ДОЛГИ";

// Table Columns 
const PERSON_COLUMN_NAME = "Фамилия Имя";
const PERSON_COLUMN_LENGHT = 1;
const DATE_COLUMN_NAME = "Дата";
const DATE_COLUMN_LENGTH = 1;
const EQUIPMENT_COLUMN_NAME = "Наименование снаряжения";
const EQUIPMENT_COLUMN_LENGHT = 1;
const COUNT_COLUMN_NAME = "Количество";
const COUNT_COLUMN_LENGHT = 2;
const MAX_COLUMNS_COUNT = 15;

// Responses
const ACCESS_DENIED = {code:401, message: "Access denied"};
const UNKNOWN_ACTION = {code:404, message: "Action not found"};
const SERVER_ERROR = {code: 500, message: "Server error"};

const RESPONSE_TEMPLATE = {code: 200, message: ""};

// ACTIONS
const EQUIPMENT = "equipment";



// Use URL/exec?secret=<yoursecret>&action=<action name>
function doGet(e) {
  const accessDenied = handleAccessDenied(e);
  if (accessDenied) return HtmlService.createHtmlOutput(accessDenied);

  const action = handleAction(e);
  if (action === UNKNOWN_ACTION) return HtmlService.createHtmlOutput(JSON.stringify(UNKNOWN_ACTION));
  
  let response = action();

  return ContentService.createTextOutput(JSON.stringify(response));
}

function handleAccessDenied(e){
  if (!e || !e.parameter || !e.parameter.secret || e.parameter.secret !== SECRET) {
    return JSON.stringify(ACCESS_DENIED)
  }
  return undefined;
}

function handleAction(e){
  if (!e || !e.parameter || !e.parameter.action) {
    return UNKNOWN_ACTION;
  }
  if (e.parameter.action === EQUIPMENT) return handleEquipment;
}

function handleEquipment(){
  const storageSheet = findStoreSheet();
  if (!storageSheet) throw "Storage table is not found";

  const columnsMapping = getColumnsMapping(storageSheet);
  const takenEquipment = makeEquipmentDictionary(storageSheet, columnsMapping.equipment, columnsMapping.equipment.row, columnsMapping.count);
  
  
  
  return {
    takenEquipment
  };
}

function findStoreSheet(){
  const allSheets = SpreadsheetApp.getActive().getSheets();
  return allSheets.find(s => s.getName() === STORE_SHEET_NAME);
}

/**
 * @param {SpreadsheetApp.Sheet} storageSheet
 * @type {name: String, length: number, row: number, col: number} Column
 * @type {person: Column, date: Column, equipment: Column, count: Column} ColumnMapping
 * @returns {ColumnMapping}
 */
function getColumnsMapping(storageSheet){
  const columns = {
    person: {
      name: PERSON_COLUMN_NAME,
      length: PERSON_COLUMN_LENGHT,
      row: -1,
      col: -1
    },
    date: {
      name: DATE_COLUMN_NAME,
      length: DATE_COLUMN_LENGTH,
      row: -1,
      col: -1
    },
    equipment: {
      name: EQUIPMENT_COLUMN_NAME,
      length: EQUIPMENT_COLUMN_LENGHT,
      row: -1,
      col: -1
    },
    count: {
      name: COUNT_COLUMN_NAME,
      length: COUNT_COLUMN_LENGHT,
      row: -1,
      col: -1
    }
  };

  const colNames = Object.values(columns).map(c => c.name);
  const maxRowsCountForHeader = 10;
  let headerRow = -1;
  for (let i = 1; i < maxRowsCountForHeader+1; i++){
    const row = storageSheet.getSheetValues(i,1,1, MAX_COLUMNS_COUNT)[0];
    if (!row.find(r => colNames.includes(r))) continue;
    headerRow = i;
    break;
  }
  if (headerRow === -1) throw "Cannot find header row in table";

  const headerValues = storageSheet.getSheetValues(headerRow,1,1, MAX_COLUMNS_COUNT)[0];
  for (let i = 0; i < headerValues.length; i++){
    const columnObject = Object.values(columns).find(c => c.name == headerValues[i].toString());
    if (!columnObject) continue;
    columnObject.row = headerRow;
    if (columnObject.length === 1){
      columnObject.col = i + 1; // +1 because sheet columns start from 1
      continue;
    }
    // if column consists of several columns, let's find start point for it
    let prevColumnObject = undefined;
    for (let j = i-1; j >= 0; j--){ // try go left untill we find previous column
      prevColumnObject = Object.values(columns).find(c => c.name == headerValues[j].toString());
      if (!prevColumnObject) continue;
      break;
    }
    columnObject.col = prevColumnObject ? prevColumnObject.col + prevColumnObject.length : i + 1;
  }

  // let's verify columns
  Object.values(columns).forEach(c => {
    if (c.col === -1 || c.row === -1) throw `${c.name} is not found in sheet`;
  });

  return columns;
}

/**
 * @param {SpreadsheetApp.Sheet} storageSheet
 * @param {number} equipmentColumnIndex
 * @param {number} equipmentHeaderRowIndex
 */
function makeEquipmentDictionary(storageSheet, equipmentColumn, equipmentHeaderRowIndex, countColumn){
  let maxRows = storageSheet.getMaxRows();
  const equipmentDictionary = {};
  for (let i = equipmentHeaderRowIndex+1; i <= maxRows; i++){
    // Get Equipment name and add it to dictionary
    const equipment = storageSheet.getSheetValues(i,equipmentColumn.col,1,1)[0][0].toString();
    if (!equipment) continue;
    if (!equipmentDictionary.hasOwnProperty(equipment)) equipmentDictionary[equipment] = 0;
    // Get taken equipment
    const takenRange = storageSheet.getRange(i,countColumn.col,1,countColumn.length);
    const takenCount = getTakenEquipmentCount(takenRange);
    equipmentDictionary[equipment] += takenCount;
  }
  return equipmentDictionary;
}

/**
 * @param {SpreadsheetApp.Range} equipmentCountRange
 */
function getTakenEquipmentCount(equipmentCountRange){
  let count = 0;
  const values = equipmentCountRange.getValues();
  const styles = equipmentCountRange.getTextStyles();
  for (let i = 0; i < values.length; i++){
    for (let j = 0; j < values[i].length; j++){
      if (!styles[i][j].isStrikethrough() && values[i][j]){
        const num = Number(values[i][j].toString());
        if (num === 0 || Number.isNaN(num)) continue;
        count += num;
      }
    }
  }
  return count;
}
