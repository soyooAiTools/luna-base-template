# Luna Base Template - Tools

## stage4-to-html.cjs

将 Luna Build 的 stage4/develop 多文件输出打包成**单个 HTML 文件**，用于试玩广告投放。

### 用法

```bash
node stage4-to-html.cjs <stage4_dir> [output_dir]
```

- `stage4_dir`: Luna Build 输出的 stage4/develop 目录路径
- `output_dir`: 输出目录（默认 `./html-output`）

### 原理

1. 读取 `iframe.html` 作为基础模板（保留所有 inline scripts）
2. 遍历 stage4 所有非 JS 文件，按类型存入 `__fd` 字典：
   - JSON → 文本
   - 图片（png/jpg/webp） → base64
   - 二进制（blob/bin） → base64
3. 在 `<body>` 后注入资源拦截器（覆写 XHR/fetch/Image.src 从 fileDict 读取）
4. 在拦截器前预定义 `MODULE_*`、`DEVELOP`、`TRACE` 等全局变量
5. 正则替换 `<script src="..." defer>` → inline `<script>` 内容
6. 输出单 HTML 文件

### 注意事项

- **不依赖 Luna Playground 的 cache 目录**（直接从 stage4 原始文件打包）
- `defer` 改 inline 后执行顺序变化，需要提前定义全局变量
- `$environment.runtimeAnalysisModules` 中的所有模块需要对应的 `MODULE_*=true`
- 依赖 Node.js 内置 `zlib`（仅用于统计 gzip 大小）

### 输出示例

基准工程（无美术资源）：
- Raw: ~11.77MB
- Gzip: ~1.89MB

实际项目会根据资源量变化。渠道限制通常为 5MB gzip。
