using Microsoft.Playwright;
using NUnit.Framework;

namespace NihongoAcademy.UITests;

/// <summary>
/// Key-page smoke tests. Point APP_BASE_URL at a running instance of the app
/// (e.g. `dotnet run` -> https://localhost:5001) before running:
///   APP_BASE_URL=https://localhost:5001 dotnet test NihongoAcademy.UITests
///
/// The Chromium build is matched to the Microsoft.Playwright package revision by default.
/// If you already have a Chromium installed elsewhere (e.g. an offline mirror), point
/// CHROMIUM_PATH at its chrome.exe to skip the download:
///   CHROMIUM_PATH="C:\...\ms-playwright\chromium-XXXX\chrome-win64\chrome.exe" dotnet test ...
/// </summary>
[TestFixture]
public class SmokeTests
{
    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5000";

    private static string? ChromiumPath =>
        Environment.GetEnvironmentVariable("CHROMIUM_PATH");

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _playwright = await Playwright.CreateAsync();
        var options = new BrowserTypeLaunchOptions();
        var path = ChromiumPath;
        if (!string.IsNullOrEmpty(path))
        {
            options.ExecutablePath = path;
        }

        _browser = await _playwright.Chromium.LaunchAsync(options);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_browser != null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }

    [SetUp]
    public async Task SetUp()
    {
        _page = await _browser.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
        }
    }

    [Test]
    public async Task HomePage_Returns_200()
    {
        var response = await _page.GotoAsync(BaseUrl + "/");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task LessonsPage_Returns_200()
    {
        var response = await _page.GotoAsync(BaseUrl + "/Lessons");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task CommunityPage_Returns_200()
    {
        var response = await _page.GotoAsync(BaseUrl + "/Community");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task QuizzesPage_Redirects_Anonymous_To_Login()
    {
        await _page.GotoAsync(BaseUrl + "/Quizzes");
        Assert.That(_page.Url, Does.Contain("Account/Login").Or.Contains("/Identity/"));
    }

    [Test]
    public async Task DashboardPage_Redirects_Anonymous_To_Login()
    {
        await _page.GotoAsync(BaseUrl + "/Dashboard");
        Assert.That(_page.Url, Does.Contain("Account/Login").Or.Contains("/Identity/"));
    }
}
