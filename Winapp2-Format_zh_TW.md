# Winapp2.ini 格式規範

FluentCleaner 讀取 winapp2.ini 檔案時支援的所有設定項快速參考。

---

## 條目範例

```ini
[App Name *]
LangSecRef=3021
Detect=HKLM\Software\MyApp
DetectFile=%LocalAppData%\MyApp
SpecialDetect=DET_CHROME
Warning=This removes saved passwords
Default=False
FileKey1=%AppData%\MyApp|*.log;*.tmp
FileKey2=%AppData%\MyApp\Cache|*|REMOVESELF
RegKey1=HKCU\Software\MyApp\MRU
ExcludeKey1=FILE|%AppData%\MyApp\|important.db
```

---

## 偵測規則 (Detection)

至少需要指定一個偵測欄位 — 否則該條目會被完全隱藏。
多個 `Detect` / `DetectFile` 行採用 **或 (OR)** 邏輯，因此只要命中其中一項即算存在。

| 欄位 | 格式 | 檢查內容 |
|---|---|---|
| `Detect` | `HKLM\Software\Foo` | 登錄鍵是否存在 |
| `Detect` | `HKLM\Software\Foo\|Value` | 特定登錄值是否存在 |
| `DetectFile` | `%LocalAppData%\MyApp` | 檔案或資料夾是否存在 |
| `DetectFile` | `%LocalAppData%\Chrome*` | 路徑中可使用萬用字元（Wildcards） |
| `SpecialDetect` | `DET_CHROME` | 已知常用應用程式代碼（見下方） |

### SpecialDetect 代碼

| 代碼 | 檢查路徑/條件 |
|---|---|
| `DET_CHROME` | `%LocalAppData%\Google\Chrome\User Data` |
| `DET_FIREFOX` | `%AppData%\Mozilla\Firefox` |
| `DET_EDGE` | `%LocalAppData%\Microsoft\Edge\User Data` |
| `DET_OPERA` | `%AppData%\Opera Software\Opera Stable` |
| `DET_THUNDERBIRD` | `%AppData%\Thunderbird` |
| `DET_IE` | Internet Explorer 登錄檔路徑 |
| `DET_WINSTORE` | `%LocalAppData%\Packages` |

---

## 檔案清除規則 (FileKey)

```
FileKeyN=<path>|<pattern>[|RECURSE|REMOVESELF]
```

| 變體形式 | 範例 | 清除行為 |
|---|---|---|
| 路徑 + 模式 | `%Temp%\MyApp\|*.tmp` | 僅比對頂層檔案 |
| 多重模式 | `%Temp%\|*.log;*.tmp;*.bak` | 使用分號分隔，同時比對所有副檔名 |
| RECURSE | `%AppData%\MyApp\|*.log|RECURSE` | 遞迴比對所有子資料夾 |
| REMOVESELF | `%AppData%\MyApp\Cache\|*|REMOVESELF` | 刪除檔案後，一併清理空的資料夾 |
| 無模式僅旗標 | `%AppData%\MyApp\Cache\|REMOVESELF` | 預設為 `*.*`，旗標依然生效 |

### 路徑變數 (Path variables)

| 變數 | 解析為實體路徑 |
|---|---|
| `%AppData%` | `C:\Users\Name\AppData\Roaming` |
| `%LocalAppData%` | `C:\Users\Name\AppData\Local` |
| `%LocalLowAppData%` | `C:\Users\Name\AppData\LocalLow` |
| `%ProgramData%` / `%CommonAppData%` | `C:\ProgramData` |
| `%ProgramFiles%` | `C:\Program Files` — *亦會自動嘗試 x86 變體路徑* |
| `%ProgramFiles(x86)%` / `%ProgramFilesX86%` | `C:\Program Files (x86)` |
| `%UserProfile%` | `C:\Users\Name` |
| `%SystemRoot%` / `%WinDir%` | `C:\Windows` |
| `%System%` | `C:\Windows\System32` |
| `%Temp%` / `%Tmp%` | 使用者暫存資料夾 |
| `%SystemDrive%` | `C:` |
| `%Documents%`, `%Desktop%`, `%Music%`, `%Pictures%`, `%Videos%` | 標準系統特殊資料夾 |

路徑片段中亦支援使用萬用字元：
```
%LocalAppData%\Google\Chrome*\User Data\*\Cache
```
在掃描時，這會被擴充展開為所有相符的實體路徑。

---

## 登錄檔清除規則 (RegKey)

```
RegKeyN=<HIVE>\<path>[\|<value name>]
```

| 變體形式 | 範例 | 清除行為 |
|---|---|---|
| 整個登錄鍵 | `HKCU\Software\MyApp\MRU` | 刪除該登錄鍵及其下方所有內容 |
| 單一登錄值 | `HKCU\Software\MyApp\|LastRun` | 僅刪除該登錄值 |

支援的機碼 Hive：`HKCU`, `HKLM`, `HKU`, `HKCC`, `HKCR` — 完整格式（如 `HKEY_CURRENT_USER`）亦可解析。

---

## 排除規則 (ExcludeKey)

```
ExcludeKeyN=<TYPE>|<path>\|[<pattern>]
```

在此比對到的任何內容都會在掃描期間被跳過，即使 FileKey 原本可以匹配也會被排除。

| 類型 | 範例 | 保護對象與行為 |
|---|---|---|
| `FILE` + 完全比對名稱 | `FILE\|%AppData%\MyApp\|config.db` | 僅該特定檔案，且必須直接位於該資料夾下 |
| `FILE` + 萬用字元 | `FILE\|%AppData%\MyApp\|*.db` | 直接部位於該資料夾下的所有 `.db` 檔案 |
| `PATH` 無模式 | `PATH\|%AppData%\MyApp\Profiles\` | 整個資料夾樹狀目錄 |
| `PATH` + `*` | `PATH\|%AppData%\MyApp\_Data\|*` | 目錄樹中的每一個檔案 |
| `PATH` + 萬用字元 | `PATH\|%AppData%\MyApp\Cache\|*.db` | 遞迴包含所有目錄樹中的 `.db` 檔案 |
| `REG` | `REG\|HKCU\Software\MyApp\` | 登錄檔排除 — 在檔案掃描期間會被忽略 |

> 指定檔名的 `FILE` 僅涵蓋該資料夾的直接子項目。  
> 帶有萬用字元模式的 `PATH` 會涵蓋整個子目錄樹。

---

## 其他欄位

| 欄位 | 功能說明 |
|---|---|
| `LangSecRef` | 用於 UI 分組的類別編號（`3029` = Google Chrome 等） |
| `Section` | 自由文字類別，當 `LangSecRef` 非已知代碼時作為備用分類 |
| `Warning` | 開始清理前向使用者顯示的警告訊息 |
| `Default` | `True` / `False` — 條目預設是否勾選 |
