# TAKT Equipment API

Сервис для информирования о состоянии склада, наличии снаряжения и т.д. Для использования требуется подключение к Google-таблице склада.

## Технологии

* .NET 7
* Swagger UI
* REST API
* PostgreSQL
* Google Apps Script

## Способ сборки

## APIv1 методы
Prefix: `host/api/v1/`

| Method | Route | Request | Response | Notes |
|--------|-------|---------|----------|-------|
| GET  | equipment/central | skip - сколько записей пропустить, take - сколько записей взять | `{items: EquipmentItemAvailability[] , totalCount: Integer}` | Возвращает массив объектов-снаряжения для центрального склада |

## APIv1 объекты

### EquipmentItemAvailability

Сущность нужна для отображения пользователю количество инвентаря на складе, с указанием какой конкретно объект и примерное количество.
```ts
class EquipmentItemAvailability {
  equipmentItemId: number, // Идентификатор вида снаряжения
  equipmentItemTitle: string, // Название, например: верёвка 30м
  storageId: number, // Идентификатор склада
  storageTitle: string, // Название склада, например: центральный склад
  availability: AvailabilityLevel, // Наличие на складе, например: AvailabilityLevel.Enough (достаточно)
  resupplyDate: Date | null // Дата пополнения запасов, если известно когда кто-то собирается вернуть эту вещь на склад
}
```

### EquipmentItem

Сущность описывает элемент снаряжения, например веревку, карабин, каску. 
```ts
class EquipmentItem {
  Id: number // Идентификатор в БД
  title: string // Отображаемое название, например: Карабин стальной
  synonyms: string[] // Синонимы, нужны для парсинга из таблиц и для поиска, например: [Кс, сталь]
}
```

### AvailabilityLevel

Перечисление нужно для описания количества снаряжения на складе без указания точного количества. 
```ts
enum AvailabilityLevel {
  Unknown = 0, // Неизветно
  Enough = 1, // Достаточно
  Medium = 2, // Средне
  Few = 3, // Мало
  OutOfStock = 4 // Отсутствует
}
```
