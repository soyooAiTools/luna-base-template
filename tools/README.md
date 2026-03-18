# Luna Stage4 → Single HTML Converter (v3)

将 Luna jake build 的 stage4 多文件输出打包为**单个可运行 HTML 文件**。

## 用法

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

## 工作原理

1. **保留 iframe.html**：不从零构建 HTML，保留 Luna 原始 iframe.html 的所有 inline scripts
2. **注入拦截器**：XHR/fetch/Image.src 拦截，通过 fileDict 返回内嵌资源（JSON/图片/blob）
3. **内联外部脚本**：将 `<script src="..." defer>` 替换为 `<script>...</script>`
4. **`\x3c` 转义**：内联 JS 中的 `<script` 和 `</script` 转义为 `\x3cscript` / `\x3c/script`，防止 HTML 解析器误识别
5. **模块剥离**：可选移除不需要的模块（TextMeshPro、DOTween 等）

## 可剥离模块参考

| 模块 | ��小 | 说明 |
|------|------|------|
| TextMeshPro | ~2.3MB | 文本渲染，基准工程不需要 |
| DOTween | ~1.2MB | 动画库，⚠️ 剥离可能导致卡加载 |
| physics2d / physics3d | ~1MB | 2D/3D 物理引擎 |
| particle-system | ~200KB | 粒子系统 |
| mecanim | ~300KB | 动画状态机 |

> ⚠️ 剥离前请确认项目是否依赖该模块，否则可能黑屏或卡加载。

## 技术决策

- **不使用 terser 压缩**：terser 会破坏 shader 字符串和 Bridge.NET 混淆代码，导致粉色材质
- **不使用旧 `worker-html-converter.js`**：旧版依赖 `cache/` 目录，jake build 不生成 cache
- **用 callback 替换而非 reverse+index**：v3 用 `String.replace(regex, callback)` 单次遍历，比 matchAll+reverse 更简洁且无 shader 问题

## 输出

- 文件名：`playable_v3.html`
- 基准工程体积（剥离 TextMeshPro）：**~8.3MB raw / ~1.5MB gzip**
- 远低于 5MB gzip 渠道限制
