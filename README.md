# AI文字自動重點整理成表格工具

## 簡介
這是一個基於 OpenAI GPT 模型的智慧文字處理工具，能夠自動將非結構化的文字內容轉換成清晰的 Markdown 表格格式，幫助使用者快速整理和理解文字資料。

## 功能特色

### 核心功能
- **智慧文字分析**：使用 OpenAI GPT 模型自動分析文字內容
- **雙向轉換**：
  - 文字→表格：將文字內容自動轉換為結構化表格
  - 表格→文字：將表格內容轉換回易讀的文字描述
- **即時 Markdown 預覽**：一鍵開啟線上預覽工具查看表格效果
- **中文最佳化**：完整支援中文內容處理

### 使用者介面
- 簡潔現代的設計風格
- 直覺的轉換操作按鈕
- 即時的狀態回饋
- 清晰的錯誤提示

## 系統需求
- Windows 作業系統
- .NET 8.0 或更高版本
- 有效的 OpenAI API Key

## 開始使用

### 安裝步驟
1. 從Release下載最新版本的壓縮檔
2. 解壓縮
3. 執行`TextTableConverter.UI.exe`

### 基本使用流程
1. 啟動應用程式
2. 輸入您的 OpenAI API Key
   - 點擊「保存」按鈕儲存Key
   - Key會安全地儲存在本機
3. 在左側文字框輸入或貼上要處理的文字
4. 使用轉換按鈕：
   - →：將文字轉換為表格
   - ←：將表格轉換回文字
5. 在右側查看轉換結果
6. 需要時可點擊「Markdown預覽」按鈕，開啟網頁後，貼上Markdown內容，查看實際表格效果

## 開發資訊

### 技術架構
- 使用者介面：WPF (Windows Presentation Foundation)
- 架構模式：MVVM (Model-View-ViewModel)
- AI 模型：OpenAI GPT-4o mini

### 主要依賴套件
- CommunityToolkit.Mvvm：MVVM 架構支援
- MaterialDesignThemes：現代化 UI 元件
- Microsoft.Extensions.DependencyInjection：依賴注入
- System.Text.Json：JSON 處理

### 專案結構
```
TextTableConverter/
├── src/
│   ├── TextTableConverter.Core/          # 核心業務邏輯
│   │   └── Services/                     # 核心服務實現
│   │       ├── Interfaces/               # 服務介面
│   │       └── TextAnalysisService.cs    # 文字分析服務
│   │
│   ├── TextTableConverter.UI/            # WPF使用者介面
│   │   ├── ViewModels/                   # 視圖模型
│   │   ├── Views/                        # XAML視圖
│   │   └── Converters/                   # 值轉換器
```

## 常見問題

### API Key相關
- **如何取得 API Key？**
  - 請前往 [OpenAI 官網](https://openai.com/) 註冊帳號
  - 在開發者設定中生成 API Key
- **Key儲存在哪裡？**
  - Key會安全地儲存在本機

### 使用限制
- 轉換速度取決於網路連線和 API 回應時間
- 建議注意 API 使用額度
- 單次轉換建議不超過 3000 字

### 錯誤排除
- **無法連線到 API**
  - 檢查網路連線
  - 確認 API Key是否有效
  - 檢查 API 使用額度是否足夠
- **轉換結果不如預期**
  - 確保輸入文字的清晰度和完整性
  - 可以嘗試調整文字結構後重試

## 版本歷程

### v1.0.0 (2024-11-17)
- 首次發布
- 實現基本的文字轉表格功能
- 支援 Markdown 預覽功能
- 支援中文介面和內容處理

## 授權說明
本軟體採用 MIT 授權條款。詳細內容請參閱 LICENSE 檔案。

## 意見回饋
如果您在使用過程中遇到任何問題，或有功能改善建議，歡迎提出 Issue。

## 開發團隊
Yanwei Liu

---
*最後更新：2024年11月17日*
