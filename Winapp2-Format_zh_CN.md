# Winapp2.ini 格式

FluentCleaner 读取 winapp2.ini 文件完整速查手册

---

## 条目示例

```ini
[App Name *]
LangSecRef=3021
Detect=HKLM\Software\MyApp
DetectFile=%LocalAppData%\MyApp
SpecialDetect=DET_CHROME
Warning=此操作会删除已保存密码
Default=False
FileKey1=%AppData%\MyApp|*.log;*.tmp
FileKey2=%AppData%\MyApp\Cache|*|REMOVESELF
RegKey1=HKCU\Software\MyApp\MRU
ExcludeKey1=FILE|%AppData%\MyApp\|important.db
```

---

## 检测规则 Detection

至少填写一条检测字段，否则该条目会完全隐藏。
多条 `Detect` / `DetectFile` 为**或逻辑**，任意一条匹配即判定软件已安装。

| 字段 | 格式 | 检测内容 |
|---|---|---|
| `Detect` | `HKLM\Software\Foo` | 检测指定注册表项是否存在 |
| `Detect` | `HKLM\Software\Foo\|Value` | 检测注册表项下指定键值是否存在 |
| `DetectFile` | `%LocalAppData%\MyApp` | 检测文件或文件夹是否存在 |
| `DetectFile` | `%LocalAppData%\Chrome*` | 路径支持通配符匹配 |
| `SpecialDetect` | `DET_CHROME` | 主流软件快捷检测标识（见下表） |

### SpecialDetect 内置标识对照表

| 标识代码 | 检测路径 |
|---|---|
| `DET_CHROME` | `%LocalAppData%\Google\Chrome\User Data` |
| `DET_FIREFOX` | `%AppData%\Mozilla\Firefox` |
| `DET_EDGE` | `%LocalAppData%\Microsoft\Edge\User Data` |
| `DET_OPERA` | `%AppData%\Opera Software\Opera Stable` |
| `DET_THUNDERBIRD` | `%AppData%\Thunderbird` |
| `DET_IE` | IE浏览器对应注册表路径 |
| `DET_WINSTORE` | `%LocalAppData%\Packages` 微软应用商店目录 |

---

## FileKey

```
FileKeyN=<路径>|<匹配规则>[|RECURSE|REMOVESELF]
```

| 写法类型 | 示例 | 执行逻辑 |
|---|---|---|
| 路径+文件通配符 | `%Temp%\MyApp\|*.tmp` | 仅匹配文件夹一级目录文件 |
| 多匹配规则 | `%Temp%\|*.log;*.tmp;*.bak` | 分号分隔，匹配全部后缀文件 |
| RECURSE 递归 | `%AppData%\MyApp\|*.log|RECURSE` | 递归遍历所有子目录匹配 |
| REMOVESELF 清理空目录 | `%AppData%\MyApp\Cache\|*|REMOVESELF` | 删除文件后自动移除空文件夹 |
| 无匹配规则仅带标记 | `%AppData%\MyApp\Cache\|REMOVESELF` | 默认匹配 `*.*`，标记依旧生效 |

### 内置路径变量对照表

| 变量名 | 实际解析路径 |
|---|---|
| `%AppData%` | `C:\Users\用户名\AppData\Roaming` |
| `%LocalAppData%` | `C:\Users\用户名\AppData\Local` |
| `%LocalLowAppData%` | `C:\Users\用户名\AppData\LocalLow` |
| `%ProgramData%` / `%CommonAppData%` | `C:\ProgramData` |
| `%ProgramFiles%` | `C:\Program Files`，程序会自动兼容x86目录 |
| `%ProgramFiles(x86)%` / `%ProgramFilesX86%` | `C:\Program Files (x86)` |
| `%UserProfile%` | `C:\Users\用户名` |
| `%SystemRoot%` / `%WinDir%` | `C:\Windows` |
| `%System%` | `C:\Windows\System32` |
| `%Temp%` / `%Tmp%` | 当前用户临时文件夹 |
| `%SystemDrive%` | 系统盘盘符，例：`C:` |
| `%Documents%`、`%Desktop%`、`%Music%`、`%Pictures%`、`%Videos%` | 系统对应常用个人文件夹 |

路径分段内支持通配符：
```
%LocalAppData%\Google\Chrome*\User Data\*\Cache
```
扫描时会自动展开匹配所有符合规则的真实目录。

---

## RegKey

```
RegKeyN=<HIVE>\<path>[\|<value name>]
```

| 写法类型 | 示例 | 执行逻辑 |
|---|---|---|
| 清理整个注册表项 | `HKCU\Software\MyApp\MRU` | 删除该项及所有下级子项、键值 |
| 仅删除单个键值 | `HKCU\Software\MyApp\|LastRun` | 只移除指定键值，保留注册表项 |

支持根项缩写：`HKCU`、`HKLM`、`HKU`、`HKCC`、`HKCR`，完整写法 `HKEY_CURRENT_USER` 等同样兼容。

---

## ExcludeKey

```
ExcludeKeyN=<类型>|<路径>\|[<匹配规则>]
```

匹配到的文件/注册表项会直接跳过，即便 FileKey/RegKey 命中也不会清理。

| 类型 | 示例 | 保护范围 |
|---|---|---|
| `FILE` 精确文件名 | `FILE\|%AppData%\MyApp\|config.db` | 仅保护文件夹一级目录内该文件 |
| `FILE` 文件通配符 | `FILE\|%AppData%\MyApp\|*.db` | 仅保护该文件夹一级目录所有db文件 |
| `PATH` 完整目录 | `PATH\|%AppData%\MyApp\Profiles\` | 保护整个目录树全部内容 |
| `PATH` 全匹配 | `PATH\|%AppData%\MyApp\_Data\|*` | 目录下所有文件递归保护 |
| `PATH` 带后缀通配符 | `PATH\|%AppData%\MyApp\Cache\|*.db` | 递归保护所有子目录内db文件 |
| `REG` 注册表排除 | `REG\|HKCU\Software\MyApp\` | 注册表项排除，文件扫描阶段忽略 |

> `FILE` 仅匹配文件夹直接子文件。 
> `PATH` 搭配通配符会覆盖整个子目录树。

---

## 其他配置字段

| 字段名 | 作用说明 |
|---|---|
| `LangSecRef` | 界面分类编号（例：`3029` 代表谷歌Chrome），用于分组展示 |
| `Section` | 自定义文本分类，当 LangSecRef 无对应内置分类时作为备用分组 |
| `Warning` | 清理前向用户弹出的风险提示文字 |
| `Default` | `True` / `False`，控制该清理项默认是否勾选
