using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PuppeteerSharp;
using System.Drawing;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Web.Http.Cors;

namespace webapi.Controllers;


[ApiController]
[Route("[controller]/[action]")]
[EnableCors(origins: "*", headers: "*", methods: "*")]
public class FilesController : ControllerBase
{
    string pathInput = Path.Combine("./files/", "input");
    string pathOutput = Path.Combine("./files/", "output");

    [HttpPost()]
    public async Task<IActionResult> Upload()
    {
        var files = Request.Form.Files;

        foreach (var file in files)
        {
            if (file.FileName.Contains(".html"))
            {
                var fileName = file.FileName;
                var path = Path.Combine("./files/", "input");
                var pathPath = Path.Combine(path, fileName);
                string errorMsg = string.Empty;
                var stream = new FileStream(pathPath, FileMode.Create);

                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    await file.CopyToAsync(stream);
                    await stream.FlushAsync();
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                finally
                {
                    stream.Close();
                }

            }
            else 
                return StatusCode(400);
        }
        return new JsonResult(files.ToList());

    }

    [HttpPost("{fiileName}")]
    public async Task<IActionResult> ConvertFile(string fiileName)
    {
        var pathPath = @$"./files/input/{fiileName}";
        string errorMsg = string.Empty;
        using var browserFetcher = new BrowserFetcher();
        IBrowser browser;
        try
        {
            await browserFetcher.DownloadAsync();
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var html = System.IO.File.ReadAllText(pathPath);

            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(html);
                var result = await page.GetContentAsync();
                await page.PdfAsync(@$"./files/output/{fiileName.Replace("html", "pdf")}");
            }
             await browser.CloseAsync();
             browser.Dispose(); 
        }
        catch (Exception ex)
        {
            errorMsg = ex.Message;
            return StatusCode(400);

        }
        finally
        {
            browserFetcher.Dispose();
        }

        return StatusCode(200);

    }

    [HttpGet()]
    public async Task<IActionResult> GetInputFiles()
    {
        string[] fileArrayInput = Directory.GetFiles(pathInput, "*.html");
        string[] fileArrayOutput = Directory.GetFiles(pathOutput);
        var HotsUrl = Request.Scheme + "://" + Request.Host.Value;

        List<FileModel> fileArrayEx = new List<FileModel>();
        foreach (string file in fileArrayInput)
        {
            var exFile = new FileModel()
            {
                Name = file.Substring(file.IndexOf("\\") + 1),
                Converted = fileArrayOutput.Contains(file.Replace("input", "output").Replace("html", "pdf")),
                Modified =  System.IO.File.GetLastWriteTime(file),
        };
            exFile.Link = exFile.Converted ? (HotsUrl + @$"/files/download/{exFile.Name.Replace("html", "pdf")}") : "don't have link";
            fileArrayEx.Add(exFile);
        }
        fileArrayEx = fileArrayEx.OrderByDescending(x => x.Modified).ToList();

            return new JsonResult(fileArrayEx.ToList());
    }

    [HttpGet("{fiileName}")]
    public async Task<IActionResult> Download(string fiileName)
    {
        var pathPath = @$"./files/output/{fiileName}";
        var fileName = System.IO.Path.GetFileName(pathPath);
        var content = await System.IO.File.ReadAllBytesAsync(pathPath);
        new FileExtensionContentTypeProvider()
            .TryGetContentType(fileName, out string contentType);
        return File(content, contentType, fileName);

    }

}