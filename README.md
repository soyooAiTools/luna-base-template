# Luna Base Template

Unity 基准工程 + Luna 7.1.0 构建模板。用于 Blueprint 试玩广告流水线的 AI 编码基础。

## 项目结构

```
luna-base-template/
├── Assets/
│   ├── Scenes/templeteScene.unity      # 基准场景
│   ├── __LunaMaterials/                # 预烘焙颜色材质（URP/Lit）
│   └── Program/Script/Manager/
│       └── GameFlowManagerMain.cs      # AI 入口文件（stub，构建时替换）
├── ProjectSettings/                     # Unity 项目设置
├── stage4-engine/                       # Luna 7.1.0 构建引擎（已打 null guard 补丁）
│   ├── scripts.js                       # 引擎 + Deserializers（~6.3MB）
│   ├── index.html                       # Luna 入口页
│   ├── luna.json                        # Luna 配置
│   └── playground.json                  # Playground 配置
└── tools/
    └── stage4-to-html.cjs              # [历史] 独立打包器，现已集成到 linux-bridge-build.js
```

## 场景内容

### 预烘焙颜色池（160 个 3D 对象）

颜色直接烘焙在 Unity 材质中（URP/Lit shader），无需运行时 SetColor。

| 形状 | 每色数量 | 命名 |
|------|---------|------|
| Cube | 5 | `__Pool_Cube_{Color}_01` ~ `05` |
| Sphere | 5 | `__Pool_Sphere_{Color}_01` ~ `05` |
| Cylinder | 3 | `__Pool_Cylinder_{Color}_01` ~ `03` |
| Plane | 3 | `__Pool_Plane_{Color}_01` ~ `03` |

**10 种颜色**：Red, Blue, Green, Yellow, Orange, Purple, White, Brown, Cyan, Pink

共 (5+5+3+3) x 10 = **160 个对象**，初始位于 `(0, -999, 0)`（不可见）。

### 基础设施对象

| 对象 | 用途 |
|------|------|
| `__MainLight` | 方向光 |
| `__Ground` | 地面（10x10 Plane，Brown 材质） |
| `__MaterialSource` | 干净 URP/Lit 材质源（无纹理采样器，防止 GL_INVALID_OPERATION） |
| `Canvas` + `EventSystem` | UI 系统 |
| `GameManager` | 挂载 GameFlowManagerMain 脚本 |

## 技术规格

- **Unity**: 2022.3.14f1c1
- **Luna**: 7.1.0（单文件 engine/scripts.js 格式，含内置 Deserializers）
- **渲染管线**: URP (Universal Render Pipeline)
- **材质**: URP/Lit shader，颜色通过 `_BaseColor` 属性烘焙

## 构建方式

不再需要 Windows 手动构建。当前使用 Linux ECS 自动化流水线：

```
C# 源码 → MSBuild + Bridge.NET 编译 → JS
→ 拼接到 stage4 engine/scripts.js（去除 stub GameFlowManagerMain）
→ convertToSingleHTML 打包为单文件 HTML（~7MB）
```

构建服务：`/opt/blueprint-editor/worker/linux-bridge-build.js`（端口 3080）

## 引擎补丁说明

`stage4-engine/scripts.js` 包含以下补丁（2026-04-02）：

1. **loadSettings null guards** — 7 个设置加载函数添加空值保护，防止项目设置缺失时引擎崩溃（黑屏根因之一）
   - loadScriptsExecutionOrderSettings, loadSortingLayerSettings, loadCullingLayerSettings
   - loadTimeSettings, loadPhysicsSettings, loadPhysics2DSettings, loadQualitySettings

2. **stub GameFlowManagerMain 剥离** — 构建时自动从模板引擎中移除 stub 类定义，防止与用户代码冲突导致 "Class already defined" 异常（黑屏根因之二）

3. **_invokeOverload 安全检查** — 构建时自动应用，防止 Awake 调用崩溃

## 从旧版升级

| 项目 | 旧版 (Luna 6.4.0) | 新版 (Luna 7.1.0) |
|------|-------------------|-------------------|
| 池对象 | 90 个无颜色 | 160 个预烘焙颜色 |
| 颜色 | 运行时 SetColor（GL_INVALID_OPERATION） | 材质预烘焙（零 WebGL 错误） |
| 引擎格式 | 多 JS 文件 + deserializers.js | 单文件 engine/scripts.js |
| 构建平台 | Windows + Unity GUI | Linux ECS 自动化 |

## License

Private — Soyoo AI Tools
