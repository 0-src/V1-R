# NinjaTrader Dll Client Documentation

## Overview
The `Client` dll provides methods for interacting with a trading platform via socket communication. It implements the `IClient` interface and `IDisposable` for managing resources. The class supports sending and receiving market data, placing orders, retrieving account information, and handling connectivity.

---

## Table of Contents
- [Class Variables](#class-variables)
- [Methods](#methods)
  - [AddValue](#addvalue)
  - [Ask](#ask)
  - [AskPlayback](#askplayback)
  - [AvgEntryPrice](#avgentryprice)
  - [AvgFillPrice](#avgfillprice)
  - [Bid](#bid)
  - [BidPlayback](#bidplayback)
  - [BuyingPower](#buyingpower)
  - [CashValue](#cashvalue)
  - [Command](#command)
  - [ConfirmOrders](#confirmorders)
  - [Connected](#connected)
  - [Dispose](#dispose)
  - [Filled](#filled)
  - [GetDouble](#getdouble)
  - [GetInt](#getint)
  - [GetString](#getstring)
  - [Last](#last)
  - [LastPlayback](#lastplayback)
  - [MarketData](#marketdata)
  - [MarketPosition](#marketposition)
  - [NewOrderId](#neworderid)
  - [Orders](#orders)
  - [OrderStatus](#orderstatus)
  - [QueryInstrument](#queryinstrument)
  - [RealizedPnL](#realizedpnl)
  - [SetUp](#setup)
  - [SetUp (Overloaded)](#setup-overloaded)
  - [StopOrders](#stoporders)
  - [Strategies](#strategies)
  - [StrategyPosition](#strategyposition)
  - [SubscribeMarketData](#subscribemarketdata)
  - [TargetOrders](#targetorders)
  - [TearDown](#teardown)
  - [UnsubscribeMarketData](#unsubscribemarketdata)

---

## Class Variables
| Variable         | Type               | Description |
|-----------------|--------------------|-------------|
| `hadError`       | `bool`             | Tracks if an error occurred. |
| `h`             | `string`           | The host IP address. |
| `p`             | `int`              | The port number. |
| `showedError`   | `bool`             | Indicates if an error message was displayed. |
| `socket`        | `AtiSocket`        | Socket connection instance. |
| `timer`         | `System.Timers.Timer` | Timer for connection retries. |
| `values`        | `Hashtable`        | Stores key-value pairs for data retrieval. |
| `lockObject`    | `object`           | Synchronization lock object. |

---

## Methods

### `AddValue(string key, string value)`
**Description:**  
Adds or updates a key-value pair in the `values` hashtable.

**Usage Example:**
```csharp
Client client = new Client();
client.AddValue("Balance", "5000");
```

---

### `Ask(string instrument, double price, int size)`
**Description:**  
Sends an ask order for an instrument.

**Usage Example:**
```csharp
Client client = new Client();
int result = client.Ask("AAPL", 150.50, 10);
```

---

### `AskPlayback(string instrument, double price, int size, string timestamp)`
**Usage Example:**
```csharp
Client client = new Client();
int result = client.AskPlayback("AAPL", 150.50, 10, "2024-02-10T14:30:00");
```

---

### `AvgEntryPrice(string instrument, string account)`
**Usage Example:**
```csharp
Client client = new Client();
double price = client.AvgEntryPrice("AAPL", "ACC123");
```

---

### `AvgFillPrice(string orderId)`
**Usage Example:**
```csharp
double fillPrice = client.AvgFillPrice("ORD456");
```

---

### `Bid(string instrument, double price, int size)`
**Usage Example:**
```csharp
int result = client.Bid("AAPL", 149.75, 10);
```

---

### `BidPlayback(...)`
**Usage Example:**
```csharp
int result = client.BidPlayback("AAPL", 149.75, 10, "2024-02-10T14:30:00");
```

---

### `BuyingPower(string account)`
**Usage Example:**
```csharp
double power = client.BuyingPower("ACC123");
```

---

### `CashValue(string account)`
**Usage Example:**
```csharp
double cash = client.CashValue("ACC123");
```

---

### `Command(...)`
**Usage Example:**
```csharp
int result = client.Command("BUY", "ACC123", "AAPL", "MKT", 10, "LIMIT", 150.50, 0, "DAY", "", "", "", "");
```

---

### `ConfirmOrders(int confirm)`
**Usage Example:**
```csharp
int result = client.ConfirmOrders(1);
```

---

### `Connected(int showMessage)`
**Usage Example:**
```csharp
int status = client.Connected(1);
```

---

### `Dispose()`
**Usage Example:**
```csharp
client.Dispose();
```

---

### `Filled(string orderId)`
**Usage Example:**
```csharp
int filledQty = client.Filled("ORD456");
```

---

### `GetDouble(string key)`
**Usage Example:**
```csharp
double value = client.GetDouble("AvgFillPrice|ORD456");
```

---

### `GetInt(string key)`
**Usage Example:**
```csharp
int value = client.GetInt("Filled|ORD456");
```

---

### `GetString(string key)`
**Usage Example:**
```csharp
string value = client.GetString("OrderStatus|ORD456");
```

---

### `Last(string instrument, double price, int size)`
**Usage Example:**
```csharp
int result = client.Last("AAPL", 150.00, 10);
```

---

### `MarketData(string instrument, int type)`
**Usage Example:**
```csharp
double data = client.MarketData("AAPL", 0);
```

---

### `MarketPosition(string instrument, string account)`
**Usage Example:**
```csharp
int position = client.MarketPosition("AAPL", "ACC123");
```

---

### `NewOrderId()`
**Usage Example:**
```csharp
string orderId = client.NewOrderId();
```

---

### `Orders(string account)`
**Usage Example:**
```csharp
string orders = client.Orders("ACC123");
```

---

### `OrderStatus(string orderId)`
**Usage Example:**
```csharp
string status = client.OrderStatus("ORD456");
```

---

### `SubscribeMarketData(string instrument)`
**Usage Example:**
```csharp
int result = client.SubscribeMarketData("AAPL");
```

---

### `TargetOrders(string strategyId)`
**Usage Example:**
```csharp
string orders = client.TargetOrders("STRAT001");
```

---

### `TearDown()`
**Usage Example:**
```csharp
client.TearDown();
```

---

### `UnsubscribeMarketData(string instrument)`
**Usage Example:**
```csharp
int result = client.UnsubscribeMarketData("AAPL");
```
