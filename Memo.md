# 备忘录

使用软连接在 VS 目录和 Unity 目录间同步位于 Src 下的代码文件。

依赖项：BepInEx with GUI config manager

## 功能进度

- [x] 基于怪物血量长度变化的头顶血条
- [x] 随元素与暴击变化色彩的伤害跳字
- [x] Boss 血条
- [x] 硬编码 -> Config
- [ ] 切换/清空 Stub 模板
- [ ] 低优先级
  - [ ] 雷电伤害色彩
  - [ ] Stub 毒雾问题
  - [ ] 跳字水平位移
- [ ] 从文件加载字体 / 默认字体？

## 测试/试玩

- [x] 血量阈值是否合理
- [x] boss 战测试





## 已知问题（未解之谜）

- Stub 问题

  - 生成Stub时，如果不把 StubTemplate 身上的组件尽量删干净，则会导致新生成的 Stub 的神秘组件上保留有指向 StubTemplate 的引用。Stub 死亡后会发送神秘事件，影响到 StubTemplate 以及后续 Stub 生成。

    目前为了保险和方便，把 StubTemplate 的全部子 GameObject 和大多数不必要组件都删了。

  - 毒跳蚤酒的毒雾碰到 Stub 会导致大量空指针异常，并且怪物身上没有毒雾粒子效果，也不掉血。

    > 即使保留 StubTemplate 身上所有组件（但子 GameObject 还是全删），也无法避免这个问题。

    Stack trace 到 damageTag.OnBegin函数，这是调用：

    ```csharp
    damageTag.OnBegin(owner, out var spawnedLoopEffect);
    ```

    这是函数本体：

    ```csharp
    public void OnBegin(ITagDamageTakerOwner owner, out GameObject spawnedLoopEffect) {
        Vector2 tagDamageEffectPos = owner.TagDamageEffectPos;
        Vector2 original = owner.transform.TransformPoint(tagDamageEffectPos);
        if ((bool)startEffect) {
            startEffect.Spawn(original.ToVector3(startEffect.transform.localPosition.z));
        }
    
        if ((bool)tagLoopEffect) {
            spawnedLoopEffect = tagLoopEffect.Spawn(owner.transform, tagDamageEffectPos);
            spawnedLoopEffect.transform.SetPositionZ(tagLoopEffect.transform.localPosition.z);
            FollowTransform component = spawnedLoopEffect.GetComponent<FollowTransform>();
            
            if ((bool)component && component.Target != null) {
                component.Target = null;
            }
    
            ParticleEffectsLerpEmission component2 = spawnedLoopEffect.GetComponent<ParticleEffectsLerpEmission>();
            if ((bool)component2) {
                float duration = StartDelay + DelayPerHit * (float)TotalHitLimit;
                component2.Play(duration);
            }
        } else {
            spawnedLoopEffect = null;
        }
    }
    ```

    懒得修

- 野兽纹章普攻会附加一段伤害为0的攻击，不知道这是什么神秘机制

- 巨型螺蝇：血条hp不匹配，似乎和link有关

- 第四圣咏团：血条只在他俯身挥手攻击的时候显示