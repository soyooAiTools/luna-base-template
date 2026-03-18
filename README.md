# Luna Base Template

最小可用的 Luna 试玩广告基准工程。用于 Blueprint 流水线的 AI 编码基础。

## 项目结构

- `Assets/` - Unity 项目资源（最小化，已清理美术资源和游戏逻辑）
- `Assets/Scenes/templeteScene.unity` - 基准场景（包含摄像机、灯光、基本几何体）
- `ProjectSettings/` - Unity 项目设置
- `tools/` - 构建工具
  - `stage4-to-html.cjs` - Stage4 → 单 HTML 打包器

## 构建流程

1. **Unity Luna Build**：在 Unity 中打开项目 → Luna > Settings 设置 scene → Luna > Build
2. **打包单 HTML**：`node tools/stage4-to-html.cjs <LunaTemp/stage4/develop> [output_dir]`

## 技术说明

- Unity 版本：2022.3.14f1c1
- Luna 版本：6.4.0
- 场景：templeteScene（包含 Camera、Light、Cube/Sphere/Capsule）
- **Luna scene 配置必须通过 GUI 操作**（修改磁盘文件无效）
- **jake CLI 通过 SSH 不可靠**（Unity IPC 超时），建议手动 Build

## 单 HTML 打包器（stage4-to-html）

绕过 Luna Playground 的 cache 机制，直接从 stage4 原始文件打包。
详见 [tools/README.md](tools/README.md)。
