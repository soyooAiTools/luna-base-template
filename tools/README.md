# Luna Stage4 → Single HTML Converter (v3)

将 Luna jake build 的 stage4 多文件输出打包为**单个可运行 HTML 文件**。

## 完整输出流程（端到端）

### 前置条件
- Worker ECS（42.121.160.107）上安装 Node.js
- Unity 项目已完成 Luna Build（通过 Unity GUI 手动操作）
- stage4 输出目录存在：`D:\work\test-luna\Client\LunaTemp\stage4\develop\`

### Step 1: Luna Build（Unity GUI 手动）
1. 在 RDP 桌面打开 Unity Editor（2022.3.14f1c1）
2. Luna → Settings → 确认 Scene 设置正确（如 `templeteScene.unity`）
3. Luna → Build → 等待完成
4. 验证输出：`D:\work\test-luna\Client\LunaTemp\stage4\develop\` 应有 ~57 个文件（~19MB）

### Step 2: 运行 Converter
```bash
cd D:\worker-repo

# 推荐：剥离 TextMeshPro（省 ~2.3MB）
node converter-v3.cjs "D:/work/test-luna/Client/LunaTemp/stage4/develop" "D:/worker-repo/html-output" "TextMeshPro"
```

### Step 3: 验证输出
```
输出文件：D:\worker-repo\html-output\playable_v3.html
同时自动复制到桌面：C:\Users\Administrator\Desktop\playable_v3.html
```

控制台会输出：
```
FileDict entries: 19
  STRIPPED: engine/unity/bin/TextMeshPro.js
Total stripped: 2340KB

Output: D:\worker-repo\html-output\playable_v3.html
Raw: 8.30MB | Gzip: 1.50MB
Copied to desktop: playable_v3.html
```

### Step 4: 画面验证
- 双击桌面 `playable_v3.html`（file:// 协议打开）
- 确认：加载条走完 → 场景正常渲染 → 无粉色材质 → 可交互

---

## CLI 用法

```bash
node stage4-to-html.cjs <stage4目录> <输出目录> [剥离模块]
```

### 参数

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `stage4目录` | Luna stage4/develop 输出路径 | `D:/work/test-luna/Client/LunaTemp/stage4/develop` |
| `输出目录` | HTML 文件输出路径 | `D:/worker-repo/html-output` |
| `剥离模块` | 逗号分隔的模块名（可选） | 空（不剥离） |

### 示例

```bash
# 基础打包（不剥离）
node stage4-to-html.cjs "./stage4/develop" "./output"

# 剥离 TextMeshPro（推荐，省 ~2.3MB）
node stage4-to-html.cjs "./stage4/develop" "./output" "TextMeshPro"

# 剥离多个模块
node stage4-to-html.cjs "./stage4/develop" "./output" "TextMeshPro,DOTween"
```

---

## 工作原理

```
iframe.html (Luna 原始)
    │
    ├── 注入拦截器 (<body> 后)
    │   ├── 预定义全局变量 (MODULE_*, DEVELOP, TRACE...)
    │   ├── fileDict (JSON/图片/blob → base64 内嵌)
    │   └── 覆写 XHR.open/send + fetch + Image.src
    │
    ├── 内联外部脚本 (<script src="..." defer> → <script>...</script>)
    │   ├── \x3c 转义 (<script → \x3cscript, </script → \x3c/script)
    │   └── 模块剥离 (STRIP_MODULES 匹配的文件替换为空注释)
    │
    └── 输出单 HTML 文件 + gzip 统计
```

### 关键技术点

1. **保留 iframe.html**：不从零构建 HTML，保留 Luna 原始 iframe.html 的所有 inline scripts（$environment、Bridge 初始化、startGame 等）
2. **注入拦截器**：XHR/fetch/Image.src 拦截，通过 fileDict 返回内嵌资源
3. **`\x3c` 转义**：内联 JS 中的 `<script` 和 `</script` 转义，防止 HTML 解析器把 JS 字符串里的标签当成真标签
4. **预定义全局变量**：defer→inline 改变执行顺序，需提前定义 `MODULE_physics3d`、`MODULE_physics2d`、`MODULE_particle_system`、`MODULE_reflection`、`MODULE_prefabs`、`MODULE_mecanim` 等
5. **模块剥离**：匹配的 JS 文件替换为 `<script>/* STRIPPED: xxx */</script>`

---

## 模块剥离参考

| 模块 | 大小 | 可剥离 | 说明 |
|------|------|--------|------|
| TextMeshPro | ~2.3MB | ✅ 安全 | 文本渲染，基准工程不需要 |
| DOTween | ~1.2MB | ❌ 危险 | 剥离后卡加载条（引擎初始化依赖） |
| physics2d / physics3d | ~1MB | ⚠️ 需测试 | 2D/3D 物理引擎 |
| particle-system | ~200KB | ⚠️ 需测试 | 粒子系统 |
| mecanim | ~300KB | ⚠️ 需测试 | 动画状态机 |

> ⚠️ 每个模块是否可剥离**必须实际测试**，不同项目依赖不同。

---

## Stage4 文件体积分布（基准工程）

| 文件 | 大小 | 说明 |
|------|------|------|
| bridge.meta.js | ~3.1MB | Bridge.NET 反射映射元数据 |
| main.js | ~3.0MB | Luna 核心引擎 |
| TextMeshPro.js | ~2.3MB | 文本渲染（可剥离） |
| UnityEngine.js | ~2.0MB | Unity 引擎绑定 |
| bridge.js | ~1.6MB | Bridge.NET 运行时 |
| UnityEngine.UI.js | ~1.3MB | UI 系统 |
| DOTween.js | ~1.2MB | 动画库（不可剥离） |
| URP | ~0.5MB | 渲染管线 |
| physics 模块 | ~1.0MB | 物理引擎 |
| mecanim/particle | ~0.5MB | 动画/粒子 |
| fileDict (JSON/图片) | ~0.3MB | 资源数据 |
| **合计（不剥离）** | **~11.77MB** | **gzip: ~1.89MB** |
| **合计（剥离 TMP）** | **~8.30MB** | **gzip: ~1.50MB** |

---

## 技术决策记录

### 为什么不用 terser 压缩？
terser 会破坏 Luna/Unity 引擎代码中的 shader 字符串和 Bridge.NET 混淆代码，导致 **粉色材质**（WebGL shader 编译失败的 magenta fallback）。gzip 本身已提供足够压缩率（11.77MB → 1.89MB gzip），额外 JS 压缩收益极低但风险极高。

### 为什么不用旧 `worker-html-converter.js`？
旧版依赖 Luna Playground 生成的 `cache/210/` 目录（blobs.js、jsons.js、scripts.js），但 jake build 不产生 cache → 输出只有 220KB → 黑屏。

### 为什么用 callback 替换而非 reverse+index？
converter-v3-compress 使用 `matchAll` + `reverse` + `substring` index-based 替换，在不明原因下导致 shader 粉色（即使完全关闭 terser）。converter-v3 用简单的 `String.replace(regex, callback)` 单次遍历，完全正常。**简单方案优于复杂方案。**

### 为什么 fileDict 只存非 JS 文件？
JS 文件通过 `<script>` 标签内联执行，不走 XHR/fetch，所以不需要放进 fileDict。fileDict 只存 JSON（`t:'j'`）、图片（`t:'i'`, base64）、blob（`t:'b'`, base64）。

---

## 踩坑记录

| 问题 | 根因 | 解决方案 |
|------|------|---------|
| 粉色材质（magenta） | terser 破坏 shader 字符串 | 不用 terser |
| `Unexpected token '<'` | 内联 JS 含 `<script` 被 HTML 解析器识别 | `\x3c` 转义 |
| `MODULE_xxx is not defined` | defer→inline 执行顺序变化 | 预定义全局变量 |
| 黑屏（220KB 输出） | 旧 converter 依赖 cache 目录 | 改用 v3 |
| 卡加载条 | DOTween 被剥离 | 不剥离 DOTween |
| `<script src=` 残留 580 个 | 正序 String.replace 匹配到已内联内容 | 用 callback 替换 |
| converter-v3-compress 粉色 | reverse+index 替换不明问题 | 弃用，用 v3 |

---

## Git 历史

| Commit | 内容 |
|--------|------|
| `d0f2bf6` | 初始基准工程 |
| `923d371` | converter-v3 初版（stage4-to-html.cjs） |
| `5bde93d` | 加模块剥离 + `\x3c` 转义修复 + 弃用 terser + README 更新 |
