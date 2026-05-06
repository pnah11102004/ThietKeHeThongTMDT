using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using OpenQA.Selenium.Interactions;
using System.Linq;
using System.Threading;

namespace UnitTestProjectHasaki
{
    [TestClass]
    public class HasakiAddToCartTest
    {
        IWebDriver driver;
        string phone = "xxxx";
        string password = "xxxx";

        [TestInitialize]
        public void Setup()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        // HÀM LOGIN THEO LOGIC MỚI
        public void LoginModalIfNeeded(WebDriverWait wait)
        {
            try
            {
                var loginModal = driver.FindElements(By.CssSelector("div[role='dialog'][aria-label='Dialog notify form']"));
                if (loginModal.Count > 0)
                {
                    Console.WriteLine("🔐 Phát hiện modal login → tiến hành đăng nhập...");

                    var userInput = wait.Until(d => d.FindElement(By.CssSelector("input[name='username']")));
                    userInput.Clear();
                    userInput.SendKeys(phone);
                    Thread.Sleep(1000);

                    var passInput = driver.FindElement(By.CssSelector("input[name='password']"));
                    passInput.Clear();
                    passInput.SendKeys(password);
                    Thread.Sleep(1000);

                    var loginBtn = wait.Until(d => d.FindElement(By.CssSelector("button.bg-primary.rounded-full[type='submit']")));
                    loginBtn.Click();
                    Console.WriteLine(" ✓ Đã đăng nhập!");
                    Thread.Sleep(6000);
                }
            }
            catch
            {
                Console.WriteLine(" ✓ Không có popup login.");
            }
        }

        [TestMethod]
        public void AddProductBySKU()
        {
            string[] skus = { "422201448", "100220035", "222600006", "204100025", "100240016" };
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            Console.WriteLine("🤖 Bắt đầu quy trình tự động Hasaki...");
            driver.Navigate().GoToUrl("https://hasaki.vn/");
            wait.Until(drv => drv.FindElement(By.Id("search")));
            Thread.Sleep(2000);

            int ADD_TO_CART_COUNT = skus.Length;

            for (int i = 0; i < ADD_TO_CART_COUNT; i++)
            {
                var SKU = skus[i];
                Console.WriteLine($"\n=========== LẦN {i + 1}/{ADD_TO_CART_COUNT} ===========");
                Console.WriteLine($"🔍 Tìm kiếm SKU: {SKU}");

                // ===== 1. Tìm kiếm =====
                var searchInput = wait.Until(drv => drv.FindElement(By.Id("search")));
                searchInput.Clear();
                searchInput.SendKeys(SKU + Keys.Enter);
                Thread.Sleep(3000);

                // ===== 2. Click sản phẩm đầu tiên =====
                Console.WriteLine("🖱️ Chọn sản phẩm đầu tiên...");
                var firstProduct = wait.Until(drv => drv.FindElements(By.CssSelector(".grid.grid-cols-4.gap-2\\.5.px-2\\.5.mt-5 > div a")).FirstOrDefault());
                if (firstProduct == null)
                {
                    Console.WriteLine($"⚠️ Không tìm thấy sản phẩm với SKU {SKU}");
                    continue;
                }
                firstProduct.Click();
                Thread.Sleep(3000);

                // ===== 3. Thêm vào giỏ hàng =====
                void ClickAddToCart()
                {
                    var addToCartButton = wait.Until(drv =>
                    {
                        var btn = drv.FindElement(By.CssSelector("button.bg-orange"));
                        return btn.Displayed ? btn : null;
                    });
                    ((IJavaScriptExecutor)driver).ExecuteScript(
                        "arguments[0].scrollIntoView({behavior:'smooth',block:'center'});", addToCartButton);
                    Thread.Sleep(1000);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", addToCartButton);
                    Thread.Sleep(2000);
                }

                Console.WriteLine("🛒 Thêm vào giỏ hàng lần 1...");
                ClickAddToCart();

                // ===== 3.1 Xử lý login bằng modal (nếu xuất hiện) =====
                if (i == 0) // chỉ login lần đầu
                {
                    LoginModalIfNeeded(wait);
                    Console.WriteLine("🛒 Thêm vào giỏ hàng lần 2 sau khi đăng nhập...");
                    ClickAddToCart(); // thêm lại sau khi login
                }
                else
                {
                    Console.WriteLine(" ✓ Bỏ qua login, đã đăng nhập trước đó.");
                }

                Console.WriteLine($" ✓ Đã thêm vào giỏ hàng (Lần {i + 1})");
                Thread.Sleep(2000);

                // ===== 4. Quay về home để lặp =====
                if (i < ADD_TO_CART_COUNT - 1)
                {
                    Console.WriteLine("↩️ Quay lại trang chủ...");
                    driver.Navigate().GoToUrl("https://hasaki.vn/");
                    wait.Until(drv => drv.FindElement(By.Id("search")));
                    Thread.Sleep(2000);
                }
            }

            // ===== 5. Xem giỏ hàng =====
            Console.WriteLine("\n🛍️ Mở giỏ hàng để kiểm tra...");
            driver.Navigate().GoToUrl("https://hasaki.vn/checkout/cart");
            wait.Until(drv => drv.FindElements(By.CssSelector(".cart-item")).Count > 0);
            Thread.Sleep(3000);
            Console.WriteLine(" ✓ Giỏ hàng đã hiển thị.");

            Console.WriteLine($"\n🎉 HOÀN TẤT: Thêm giỏ hàng {ADD_TO_CART_COUNT} lần!");
            Thread.Sleep(5000);
        }

        [TestCleanup]
        public void Close()
        {
            driver.Quit();
        }
    }
}
