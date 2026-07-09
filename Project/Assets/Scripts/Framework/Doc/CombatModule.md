# 战斗模块使用说明

> 本文介绍 UnitFramework 的战斗系统，包括战斗者（Combator）、战斗事件、能力（Ability）与效果处理。

---

## 1. 模块入口

战斗模块入口为 [`CombatSystem`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat/CombatSystem.cs)。

快捷访问：

```csharp
GameApp.CombatSystem
```

---

## 2. 核心概念

### 2.1 Combator（战斗者）

[`Combator`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat/Combator.cs) 是战斗参与者的核心对象，包含：

- `AttributeBox`：属性系统
- `CombatEffectManager`：效果管理
- `Context`：数据上下文
- 子能力（ChildAbilities）与挂载能力（MoutAbilities）

### 2.2 CombatEvent（战斗事件）

[`CombatEvent`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat/CombatEvent.cs) 是战斗系统内部的事件对象，支持：

- `eventCode`：事件码
- `sender`：发送者
- `Targets`：目标列表
- `DataManager`：事件上下文数据
- `RetCode32`：返回码
- `CallBack()`：回调机制

### 2.3 Ability（能力）

能力是挂载在 Combator 上的技能/状态/特性，支持主动触发、被动响应事件。

---

## 3. 创建战斗者

```csharp
Combator hero = CombatSystem.Ins.CreateCombator();
hero.Active = true;
```

---

## 4. 创建与发送战斗事件

### 4.1 创建事件

```csharp
CombatEvent evt = hero.CreateEvent(CombatCode.OnAttack);
evt.AddTarget(enemy);
evt.SetContext("Damage", 100);
```

### 4.2 发送事件

```csharp
hero.SendEvent(evt);
```

### 4.3 事件码

常用事件码在 [`CombatCode`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat/Configs/CombatCode.cs) 中定义：

```csharp
public static class CombatCode
{
    public const string OnAbilityUpdate = "OnAbilityUpdate";
    public const string OnAttack = "OnAttack";
    public const string OnHit = "OnHit";
    public const string OnDamage = "OnDamage";
    // ...
}
```

---

## 5. 属性系统

### 5.1 设置属性

```csharp
hero.AttributeBox.SetAttrValue("HP", 1000);
hero.AttributeBox.SetAttrValue("ATK", 100);
```

### 5.2 读取属性

```csharp
float hp = hero.AttributeBox.GetAttrValue("HP");
```

### 5.3 属性公式

`NumericFormula` 支持属性之间的公式计算。

---

## 6. 效果处理

### 6.1 添加效果标签

```csharp
hero.AddTag(new EffectTag("Burning"));
```

### 6.2 检查效果

```csharp
hero.CheckEffect();
```

`CombatEffectManager` 会根据当前标签触发对应效果。

---

## 7. 完整示例

```csharp
public class SkillAbility : Ability
{
    protected override void OnEventReceive(CombatEvent combatEvent)
    {
        if (combatEvent.IsCode(CombatCode.OnAttack))
        {
            int damage = combatEvent.GetContext<int>("Damage");
            damage += Master.AttributeBox.GetAttrValue("ATK");
            combatEvent.SetContext("Damage", damage);
        }
    }
}

// 使用
Combator hero = CombatSystem.Ins.CreateCombator();
hero.BirthAbility(new SkillAbility());

CombatEvent attack = hero.CreateEvent(CombatCode.OnAttack);
attack.AddTarget(enemy);
attack.SetContext("Damage", 50);
hero.SendEvent(attack);
```

---

## 8. 最佳实践

- 所有战斗逻辑通过 `CombatEvent` 驱动，便于扩展和调试。
- 技能、Buff、Debuff 都实现为 `Ability` 子类。
- 属性计算使用 `AttributeBox` 和 `NumericFormula`，避免硬编码。
