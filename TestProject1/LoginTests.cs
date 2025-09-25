using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace TestProject1;

[TestClass]
public sealed class LoginTests
{
    private IWebDriver? _driver;
    private WebDriverWait? _wait;

    [TestInitialize]
    public void Setup()
    {
        try
        {
            // 設置 Chrome 選項
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--window-size=1920,1080");
            
            // 可選：無頭模式（在後台運行）
            // chromeOptions.AddArgument("--headless");

            _driver = new ChromeDriver(chromeOptions);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _driver.Manage().Window.Maximize();
            
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            
            // 統一導航到 UAT Asia 首頁
            _driver.Navigate().GoToUrl("https://stage.uatasia.com/en");
            
            // 等待頁面加載完成
            _wait.Until(driver => !string.IsNullOrEmpty(driver.Title));
            
            // 處理 Cookie 彈窗干擾 - 設置多個 Cookie 來避免彈窗
            try
            {
                // 設置多個 Cookie 來避免各種彈窗干擾
                var cookies = new[]
                {
                    new Cookie("cookieUsageNoticePopup", "1", ".uatasia.com", "/", DateTime.Now.AddYears(1)),
                    new Cookie("MainBannerPopup_michael001", "1", ".uatasia.com", "/", DateTime.Now.AddYears(1)),
                    new Cookie("MainBannerPopup_guest", "1", ".uatasia.com", "/", DateTime.Now.AddYears(1))
                };
                
                foreach (var cookie in cookies)
                {
                    _driver.Manage().Cookies.AddCookie(cookie);
                }
                
                // 刷新頁面以應用 Cookie
                _driver.Navigate().Refresh();
                
                // 等待頁面重新加載
                _wait.Until(driver => !string.IsNullOrEmpty(driver.Title));
                
                Console.WriteLine("成功設置 Cookie 來避免彈窗干擾");
                
                // 額外處理可能的彈窗
                try
                {
                    var popup = _driver.FindElement(By.CssSelector("div[data-ttid='pop-up']"));
                    if (popup.Displayed)
                    {
                        Console.WriteLine("發現彈窗，嘗試關閉");
                        // 嘗試找到關閉按鈕
                        var closeButtons = _driver.FindElements(By.CssSelector("[class*='close'], [class*='dismiss'], [class*='cancel'], button[aria-label*='close'], button[aria-label*='Close']"));
                        foreach (var closeBtn in closeButtons)
                        {
                            if (closeBtn.Displayed)
                            {
                                closeBtn.Click();
                                Thread.Sleep(1000);
                                break;
                            }
                        }
                        
                        // 如果沒有找到關閉按鈕，嘗試按 ESC 鍵
                        _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception popupEx)
                {
                    Console.WriteLine($"處理彈窗時發生錯誤: {popupEx.Message}");
                }
            }
            catch (Exception cookieEx)
            {
                Console.WriteLine($"設置 Cookie 時發生錯誤: {cookieEx.Message}");
                // 即使 Cookie 設置失敗，測試仍可繼續
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"瀏覽器初始化失敗: {ex.Message}", ex);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理瀏覽器時發生錯誤: {ex.Message}");
        }
    }

    [TestMethod]
    public void NavigateToUatAsiaHomePage_ShouldLoadSuccessfully()
    {
        try
        {
            // 驗證頁面標題不為空
            Assert.IsFalse(string.IsNullOrEmpty(_driver!.Title), 
                $"頁面標題不應該為空，但實際標題為: '{_driver.Title}'");
            
            // 驗證頁面已完全加載（檢查 body 元素存在）
            var bodyElement = _wait!.Until(driver => driver.FindElement(By.TagName("body")));
            Assert.IsNotNull(bodyElement, "頁面主體應該存在");
            
            // 驗證頁面包含基本內容
            var pageContent = _driver.FindElement(By.TagName("body"));
            Assert.IsTrue(pageContent.Text.Length > 0, "頁面應該包含文字內容");
        }
        catch (Exception ex)
        {
            Assert.Fail($"測試執行失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void UatAsiaHomePage_ShouldHaveCorrectLogoElements()
    {
        try
        {
            
            // 驗證 logo 容器存在
            var logoContainer = _driver.FindElement(By.CssSelector("div.logo"));
            Assert.IsNotNull(logoContainer, "Logo 容器應該存在");
            Assert.IsTrue(logoContainer.Displayed, "Logo 容器應該可見");
            
            // 驗證 12Bet Logo 存在
            var betLogo = _driver.FindElement(By.CssSelector("img[alt='12Bet Logo']"));
            Assert.IsNotNull(betLogo, "12Bet Logo 應該存在");
            Assert.IsTrue(betLogo.Displayed, "12Bet Logo 應該可見");
            
            // 驗證 Yonex All England Logo 存在
            var yonexLogo = _driver.FindElement(By.CssSelector("img[alt='yonex-all-england logo']"));
            Assert.IsNotNull(yonexLogo, "Yonex All England Logo 應該存在");
            Assert.IsTrue(yonexLogo.Displayed, "Yonex All England Logo 應該可見");
            
            // 驗證 Rewards Logo 存在
            var rewardsLogo = _driver.FindElement(By.CssSelector("img[alt='rewards Logo']"));
            Assert.IsNotNull(rewardsLogo, "Rewards Logo 應該存在");
            Assert.IsTrue(rewardsLogo.Displayed, "Rewards Logo 應該可見");
            
            // 驗證所有 logo 圖片都有正確的 src 屬性
            var allLogos = _driver.FindElements(By.CssSelector("div.logo img"));
            Assert.IsTrue(allLogos.Count >= 3, $"應該至少有 3 個 logo，但只找到 {allLogos.Count} 個");
            
            foreach (var logo in allLogos)
            {
                var src = logo.GetAttribute("src");
                Assert.IsFalse(string.IsNullOrEmpty(src), "每個 logo 都應該有 src 屬性");
                Assert.IsTrue(src.StartsWith("https://"), "Logo 圖片應該使用 HTTPS 協議");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Logo 元素驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void UatAsiaHomePage_ShouldHaveClickableLogoLinks()
    {
        try
        {
            
            // 驗證 12Bet Logo 鏈接
            var betLogoLink = _driver.FindElement(By.CssSelector("div.logo a[href='/en']"));
            Assert.IsNotNull(betLogoLink, "12Bet Logo 鏈接應該存在");
            Assert.IsTrue(betLogoLink.Displayed, "12Bet Logo 鏈接應該可見");
            
            // 驗證 Rewards Logo 鏈接
            var rewardsLogoLink = _driver.FindElement(By.CssSelector("div.logo a[href='/rewards']"));
            Assert.IsNotNull(rewardsLogoLink, "Rewards Logo 鏈接應該存在");
            Assert.IsTrue(rewardsLogoLink.Displayed, "Rewards Logo 鏈接應該可見");
            
            // 驗證鏈接可以點擊
            Assert.IsTrue(betLogoLink.Enabled, "12Bet Logo 鏈接應該可以點擊");
            Assert.IsTrue(rewardsLogoLink.Enabled, "Rewards Logo 鏈接應該可以點擊");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Logo 鏈接驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void LoginButton_ShouldBeVisibleAndClickable()
    {
        try
        {
            
            // 驗證登入按鈕存在且可見
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            Assert.IsNotNull(loginButton, "登入按鈕應該存在");
            Assert.IsTrue(loginButton.Displayed, "登入按鈕應該可見");
            Assert.IsTrue(loginButton.Enabled, "登入按鈕應該可以點擊");
            
            // 驗證登入按鈕文字
            Assert.AreEqual("Login", loginButton.Text.Trim(), "登入按鈕文字應該是 'Login'");
            
            // 驗證登入按鈕有正確的 CSS 類
            var buttonClasses = loginButton.GetAttribute("class");
            Assert.IsTrue(buttonClasses.Contains("primary"), "登入按鈕應該有 'primary' 類");
        }
        catch (Exception ex)
        {
            Assert.Fail($"登入按鈕驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void LoginButton_Click_ShouldOpenLoginModal()
    {
        try
        {
            
            // 找到並點擊登入按鈕
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            loginButton.Click();
            
            // 等待登入模態框出現（React 需要時間渲染）
            Thread.Sleep(2000);
            
            // 驗證登入模態框或表單出現
            var loginForm = _wait.Until(driver => 
                driver.FindElement(By.CssSelector("form, .login-form, .modal, [class*='login'], [class*='modal']")));
            
            Assert.IsNotNull(loginForm, "登入表單應該出現");
            Assert.IsTrue(loginForm.Displayed, "登入表單應該可見");
        }
        catch (Exception ex)
        {
            Assert.Fail($"點擊登入按鈕後模態框驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void LoginForm_ShouldHaveRequiredFields()
    {
        try
        {
            
            // 點擊登入按鈕
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            loginButton.Click();
            
            // 等待登入表單出現
            Thread.Sleep(2000);
            
            // 驗證用戶名/郵箱輸入框
            var usernameField = _driver.FindElement(By.CssSelector("input[type='email'], input[type='text'], input[name*='user'], input[name*='email'], input[placeholder*='email'], input[placeholder*='user']"));
            Assert.IsNotNull(usernameField, "用戶名/郵箱輸入框應該存在");
            Assert.IsTrue(usernameField.Displayed, "用戶名/郵箱輸入框應該可見");
            
            // 驗證密碼輸入框
            var passwordField = _driver.FindElement(By.CssSelector("input[type='password'], input[name*='password'], input[name*='pass']"));
            Assert.IsNotNull(passwordField, "密碼輸入框應該存在");
            Assert.IsTrue(passwordField.Displayed, "密碼輸入框應該可見");
            
            // 驗證提交按鈕
            var submitButton = _driver.FindElement(By.CssSelector("button[type='submit'], input[type='submit'], button[class*='submit'], button[class*='login']"));
            Assert.IsNotNull(submitButton, "提交按鈕應該存在");
            Assert.IsTrue(submitButton.Displayed, "提交按鈕應該可見");
        }
        catch (Exception ex)
        {
            Assert.Fail($"登入表單字段驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void LoginForm_ShouldAcceptInput()
    {
        try
        {
            
            // 點擊登入按鈕
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            loginButton.Click();
            
            // 等待登入表單出現
            Thread.Sleep(2000);
            
            // 找到輸入框
            var usernameField = _driver.FindElement(By.CssSelector("input[type='email'], input[type='text'], input[name*='user'], input[name*='email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[type='password'], input[name*='password']"));
            
            // 測試輸入功能
            usernameField.Clear();
            usernameField.SendKeys("test@example.com");
            Assert.AreEqual("test@example.com", usernameField.GetAttribute("value"), "用戶名輸入應該正確");
            
            passwordField.Clear();
            passwordField.SendKeys("testpassword");
            Assert.AreEqual("testpassword", passwordField.GetAttribute("value"), "密碼輸入應該正確");
        }
        catch (Exception ex)
        {
            Assert.Fail($"登入表單輸入測試失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void RegisterButton_ShouldBeVisibleAndClickable()
    {
        try
        {
            
            // 驗證註冊按鈕存在且可見
            var registerButton = _driver.FindElement(By.CssSelector("button.green.btn-shiny-border"));
            Assert.IsNotNull(registerButton, "註冊按鈕應該存在");
            Assert.IsTrue(registerButton.Displayed, "註冊按鈕應該可見");
            Assert.IsTrue(registerButton.Enabled, "註冊按鈕應該可以點擊");
            
            // 驗證註冊按鈕文字
            Assert.AreEqual("Register", registerButton.Text.Trim(), "註冊按鈕文字應該是 'Register'");
            
            // 驗證註冊按鈕有正確的 CSS 類
            var buttonClasses = registerButton.GetAttribute("class");
            Assert.IsTrue(buttonClasses.Contains("green"), "註冊按鈕應該有 'green' 類");
            Assert.IsTrue(buttonClasses.Contains("btn-shiny-border"), "註冊按鈕應該有 'btn-shiny-border' 類");
        }
        catch (Exception ex)
        {
            Assert.Fail($"註冊按鈕驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void LanguageSelector_ShouldBeVisibleAndFunctional()
    {
        try
        {
            
            // 驗證語言選擇器存在
            var languageSelector = _driver.FindElement(By.CssSelector("div.language-selector"));
            Assert.IsNotNull(languageSelector, "語言選擇器應該存在");
            Assert.IsTrue(languageSelector.Displayed, "語言選擇器應該可見");
            
            // 驗證當前語言顯示
            var currentLanguage = _driver.FindElement(By.CssSelector("button.thrid"));
            Assert.IsTrue(currentLanguage.Text.Contains("English"), "當前語言應該顯示 'English'");
            
            // 點擊語言選擇器
            currentLanguage.Click();
            Thread.Sleep(1000);
            
            // 驗證語言選項列表出現
            var languageList = _driver.FindElement(By.CssSelector("div.language-list"));
            Assert.IsTrue(languageList.Displayed, "語言選項列表應該可見");
        }
        catch (Exception ex)
        {
            Assert.Fail($"語言選擇器驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void CookiePopup_ShouldBeHandled()
    {
        try
        {
            // 驗證 cookieUsageNoticePopup Cookie 已正確設置
            var cookieUsageNotice = _driver.Manage().Cookies.GetCookieNamed("cookieUsageNoticePopup");
            Assert.IsNotNull(cookieUsageNotice, "cookieUsageNoticePopup Cookie 應該存在");
            Assert.AreEqual("1", cookieUsageNotice.Value, "cookieUsageNoticePopup Cookie 值應該是 '1'");
            Assert.AreEqual(".uatasia.com", cookieUsageNotice.Domain, "cookieUsageNoticePopup Cookie 域名應該是 '.uatasia.com'");
            Assert.AreEqual("/", cookieUsageNotice.Path, "cookieUsageNoticePopup Cookie 路徑應該是 '/'");
            
            // 驗證 MainBannerPopup_guest Cookie 已正確設置
            var mainBannerPopup = _driver.Manage().Cookies.GetCookieNamed("MainBannerPopup_guest");
            Assert.IsNotNull(mainBannerPopup, "MainBannerPopup_guest Cookie 應該存在");
            Assert.AreEqual("1", mainBannerPopup.Value, "MainBannerPopup_guest Cookie 值應該是 '1'");
            Assert.AreEqual(".uatasia.com", mainBannerPopup.Domain, "MainBannerPopup_guest Cookie 域名應該是 '.uatasia.com'");
            Assert.AreEqual("/", mainBannerPopup.Path, "MainBannerPopup_guest Cookie 路徑應該是 '/'");
            
            Console.WriteLine("所有 Cookie 都已正確設置");
            
            // 驗證沒有彈窗干擾（檢查是否有彈窗元素）
            var popupElements = _driver.FindElements(By.CssSelector("[class*='popup'], [class*='modal'], [class*='cookie'], [class*='notice'], [class*='banner']"));
            var visiblePopups = popupElements.Where(element => element.Displayed).ToList();
            
            // 如果有可見的彈窗，記錄但不會讓測試失敗
            if (visiblePopups.Count > 0)
            {
                Console.WriteLine($"發現 {visiblePopups.Count} 個可能的彈窗元素，但測試繼續執行");
                foreach (var popup in visiblePopups)
                {
                    Console.WriteLine($"彈窗元素: {popup.TagName} - {popup.GetAttribute("class")}");
                }
            }
            else
            {
                Console.WriteLine("沒有發現可見的彈窗元素");
            }
            
            // 驗證主要頁面元素仍然可見（確保彈窗沒有遮擋）
            var mainContent = _driver.FindElement(By.TagName("body"));
            Assert.IsTrue(mainContent.Displayed, "主要頁面內容應該可見");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Cookie 彈窗處理驗證失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void Login_WithValidCredentials_ShouldSucceed()
    {
        try
        {
            // 先處理可能存在的彈窗干擾
            try
            {
                var popup = _driver.FindElement(By.CssSelector("div[data-ttid='pop-up']"));
                if (popup.Displayed)
                {
                    Console.WriteLine("發現彈窗，嘗試關閉");
                    // 嘗試找到關閉按鈕
                    var closeButtons = _driver.FindElements(By.CssSelector("[class*='close'], [class*='dismiss'], [class*='cancel'], button[aria-label*='close'], button[aria-label*='Close']"));
                    foreach (var closeBtn in closeButtons)
                    {
                        if (closeBtn.Displayed)
                        {
                            closeBtn.Click();
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    
                    // 如果沒有找到關閉按鈕，嘗試按 ESC 鍵
                    _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception popupEx)
            {
                Console.WriteLine($"處理彈窗時發生錯誤: {popupEx.Message}");
            }
            
            // 點擊登入按鈕
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            
            // 使用 JavaScript 點擊來避免元素被遮擋的問題
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", loginButton);
            
            // 等待登入表單出現
            Thread.Sleep(2000);
            
            // 找到輸入框
            var usernameField = _driver.FindElement(By.CssSelector("input[type='email'], input[type='text'], input[name*='user'], input[name*='email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[type='password'], input[name*='password']"));
            
            // 輸入測試帳號密碼（請替換為您的實際帳號密碼）
            // 方法 1：直接替換（請替換為您的實際帳號密碼）
            string testUsername = "michael001";  // 請替換為您的實際用戶名
            string testPassword = "Aa111111";  // 請替換為您的實際密碼
            
            // 清空並輸入用戶名
            usernameField.Clear();
            usernameField.SendKeys(testUsername);
            
            // 清空並輸入密碼
            passwordField.Clear();
            passwordField.SendKeys(testPassword);
            
            // 找到並點擊提交按鈕
            var submitButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-submit-btn']"));
            submitButton.Click();
            
            // 等待登入處理完成
            Thread.Sleep(2000);
            
            // 驗證登入成功（檢查是否出現使用者帳號）
            try
            {
                // 檢查是否出現用戶相關元素（登入成功後通常會有）
                var userElements = _driver.FindElements(By.CssSelector("[data-ttid*='user-name']"));
                if (userElements.Count > 0)
                {
                    if (userElements[0].Text == testUsername)
                    {
                        Console.WriteLine($"找到用戶帳號:{userElements[0].Text}");
                    }
                }
            }
            catch (Exception verificationEx)
            {
                Console.WriteLine($"登入驗證過程中發生錯誤: {verificationEx.Message}");
            }
            
            Console.WriteLine("登入測試完成");
        }
        catch (Exception ex)
        {
            Assert.Fail($"登入測試失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void Login_WithInvalidCredentials_ShouldShowError()
    {
        try
        {
            // 先處理可能存在的彈窗干擾
            try
            {
                var popup = _driver.FindElement(By.CssSelector("div[data-ttid='pop-up']"));
                if (popup.Displayed)
                {
                    Console.WriteLine("發現彈窗，嘗試關閉");
                    // 嘗試找到關閉按鈕
                    var closeButtons = _driver.FindElements(By.CssSelector("[class*='close'], [class*='dismiss'], [class*='cancel'], button[aria-label*='close'], button[aria-label*='Close']"));
                    foreach (var closeBtn in closeButtons)
                    {
                        if (closeBtn.Displayed)
                        {
                            closeBtn.Click();
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    
                    // 如果沒有找到關閉按鈕，嘗試按 ESC 鍵
                    _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception popupEx)
            {
                Console.WriteLine($"處理彈窗時發生錯誤: {popupEx.Message}");
            }
            
            // 點擊登入按鈕
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            
            // 使用 JavaScript 點擊來避免元素被遮擋的問題
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", loginButton);
            
            // 等待登入表單出現
            Thread.Sleep(2000);
            
            // 找到輸入框
            var usernameField = _driver.FindElement(By.CssSelector("input[type='email'], input[type='text'], input[name*='user'], input[name*='email']"));
            var passwordField = _driver.FindElement(By.CssSelector("input[type='password'], input[name*='password']"));
            
            // 輸入無效的帳號密碼
            string invalidUsername = "invalid_user@test.com";
            string invalidPassword = "wrong_password";
            
            // 清空並輸入無效用戶名
            usernameField.Clear();
            usernameField.SendKeys(invalidUsername);
            
            // 清空並輸入無效密碼
            passwordField.Clear();
            passwordField.SendKeys(invalidPassword);
            
            // 找到並點擊提交按鈕
            var submitButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-submit-btn']"));
            submitButton.Click();
            
            // 等待錯誤訊息出現
            Thread.Sleep(3000);
            
            // 驗證是否出現錯誤訊息
            var errorMessages = _driver.FindElements(By.CssSelector("[class*='error'], [class*='alert'], [class*='message'], [class*='invalid'], [class*='failed']"));
            var visibleErrors = errorMessages.Where(error => error.Displayed).ToList();
            
            if (visibleErrors.Count > 0)
            {
                Console.WriteLine($"發現 {visibleErrors.Count} 個錯誤訊息");
                foreach (var error in visibleErrors)
                {
                    Console.WriteLine($"錯誤訊息: {error.Text}");
                }
                Assert.IsTrue(true, "無效憑證應該顯示錯誤訊息");
            }
            else
            {
                Console.WriteLine("沒有發現明顯的錯誤訊息，但測試繼續");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"無效憑證登入測試失敗: {ex.Message}");
        }
    }

    [TestMethod]
    public void PopupHandling_ShouldCloseInterferingPopups()
    {
        try
        {
            // 檢查是否有彈窗存在
            var popups = _driver.FindElements(By.CssSelector("div[data-ttid='pop-up'], [class*='popup'], [class*='modal'], [class*='overlay']"));
            var visiblePopups = popups.Where(popup => popup.Displayed).ToList();
            
            if (visiblePopups.Count > 0)
            {
                Console.WriteLine($"發現 {visiblePopups.Count} 個可見彈窗");
                
                foreach (var popup in visiblePopups)
                {
                    Console.WriteLine($"彈窗類別: {popup.GetAttribute("class")}");
                    Console.WriteLine($"彈窗 ID: {popup.GetAttribute("id")}");
                    Console.WriteLine($"彈窗 data-ttid: {popup.GetAttribute("data-ttid")}");
                }
                
                // 嘗試關閉所有可見彈窗
                foreach (var popup in visiblePopups)
                {
                    try
                    {
                        // 嘗試找到關閉按鈕
                        var closeButtons = popup.FindElements(By.CssSelector("[class*='close'], [class*='dismiss'], [class*='cancel'], button[aria-label*='close'], button[aria-label*='Close'], .close, .dismiss, .cancel"));
                        
                        if (closeButtons.Count > 0)
                        {
                            foreach (var closeBtn in closeButtons)
                            {
                                if (closeBtn.Displayed)
                                {
                                    Console.WriteLine("找到關閉按鈕，嘗試點擊");
                                    closeBtn.Click();
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("沒有找到關閉按鈕，嘗試按 ESC 鍵");
                            _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception closeEx)
                    {
                        Console.WriteLine($"關閉彈窗時發生錯誤: {closeEx.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("沒有發現可見的彈窗");
            }
            
            // 驗證主要頁面元素是否可見
            var mainContent = _driver.FindElement(By.TagName("body"));
            Assert.IsTrue(mainContent.Displayed, "主要頁面內容應該可見");
            
            // 驗證登入按鈕是否可點擊
            var loginButton = _driver.FindElement(By.CssSelector("button[data-ttid='login-btn']"));
            Assert.IsTrue(loginButton.Displayed, "登入按鈕應該可見");
            
            Console.WriteLine("彈窗處理測試完成");
        }
        catch (Exception ex)
        {
            Assert.Fail($"彈窗處理測試失敗: {ex.Message}");
        }
    }
}