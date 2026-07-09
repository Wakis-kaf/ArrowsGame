# 消息事件系统使用说明

> 本文介绍 UnitFramework 的全局消息总线，包括订阅、派发、取消订阅与携带参数。

---

## 1. 模块入口

消息系统入口为 [`MessageDispatcher`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Event/MessageDispatcher.cs)。

快捷访问：

```csharp
GameApp.Ins.MessageDispatcher
MessageDispatcher.Ins
```

> 注意：`MessageDispatcher.Ins` 是其单例属性。

---

## 2. 核心概念

消息系统基于消息码（string）分发事件，支持 0~4 个参数。业务中通常将消息码集中定义在 [`MessageCode`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Event/MessageCode.cs) 中。

---

## 3. 订阅消息

### 3.1 无参数

```csharp
MessageDispatcher.Ins.Subscribe(MessageCode.msg_gamePlay_start, OnGamePlayStart);

private void OnGamePlayStart()
{
    // 游戏开始
}
```

### 3.2 一个参数

```csharp
MessageDispatcher.Ins.Subscribe<int>(MessageCode.msg_on_inventory_added, OnItemAdded);

private void OnItemAdded(int itemId)
{
    Debug.Log($"获得物品: {itemId}");
}
```

### 3.3 多个参数

```csharp
MessageDispatcher.Ins.Subscribe<int, int>(
    MessageCode.msg_on_item_recover_tick,
    (itemId, count) => { });
```

---

## 4. 派发消息

### 4.1 无参数

```csharp
MessageDispatcher.Ins.Dispatch(MessageCode.msg_gamePlay_start);
```

### 4.2 一个参数

```csharp
MessageDispatcher.Ins.Dispatch<int>(MessageCode.msg_on_inventory_added, 1001);
```

### 4.3 多个参数

```csharp
MessageDispatcher.Ins.Dispatch<int, int>(MessageCode.msg_on_item_recover_tick, 1001, 5);
```

---

## 5. 取消订阅

### 5.1 取消指定回调

```csharp
MessageDispatcher.Ins.Unsubscribe(MessageCode.msg_gamePlay_start, OnGamePlayStart);
MessageDispatcher.Ins.Unsubscribe<int>(MessageCode.msg_on_inventory_added, OnItemAdded);
```

### 5.2 按订阅者取消

如果订阅时传入了 `IMessageSubscriber` 实现者，可以一次性取消该对象的所有订阅：

```csharp
MessageDispatcher.Ins.UnsubscribeAll(this);
```

---

## 6. 在 Handler 中使用

业务 Handler 中常见写法：

```csharp
public class GamePlayClientHandler : GameModuleLogicHandler
{
    protected override void OnHandlerStart()
    {
        MessageDispatcher.Ins.Subscribe(MessageCode.msg_on_game_start, OnGameStart);
        MessageDispatcher.Ins.Subscribe<int>(MessageCode.msg_on_inventory_added, OnItemAdded);
    }

    protected override void OnHandlerDestroy()
    {
        MessageDispatcher.Ins.Unsubscribe(MessageCode.msg_on_game_start, OnGameStart);
        MessageDispatcher.Ins.Unsubscribe<int>(MessageCode.msg_on_inventory_added, OnItemAdded);
    }

    private void OnGameStart()
    {
        // 进入游戏
    }

    private void OnItemAdded(int itemId)
    {
        // 处理物品增加
    }
}
```

---

## 7. 完整示例

```csharp
// 在 DataHandler 中发送
public void AddItem(int itemId)
{
    inventory.Add(itemId);
    MessageDispatcher.Ins.Dispatch<int>(MessageCode.msg_on_inventory_added, itemId);
}

// 在 ViewHandler 中监听
MessageDispatcher.Ins.Subscribe<int>(MessageCode.msg_on_inventory_added, (itemId) =>
{
    Panel.OpenPanel<ItemGetPanel>();
});
```

---

## 8. 最佳实践

- 消息码集中定义在 `MessageCode` 中，避免硬编码字符串。
- 订阅后务必在 `OnHandlerDestroy` 中取消订阅，防止内存泄漏。
- 消息参数类型必须严格匹配，否则无法触发回调。
- 跨模块通信优先使用消息，避免直接引用其他模块的 Handler。
