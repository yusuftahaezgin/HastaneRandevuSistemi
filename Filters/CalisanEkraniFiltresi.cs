using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HastaneRandevuSistemi.Filters;

public class CalisanEkraniFiltresi : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        var sadeceCalisanEkrani =
            user.Identity?.IsAuthenticated == true
            && !user.IsInRole("Yonetici")
            && (user.IsInRole("Doktor")
                || user.IsInRole("SaglikPersoneli"));

        if (!sadeceCalisanEkrani)
        {
            await next();
            return;
        }

        var controller =
            context.RouteData.Values["controller"]?.ToString() ?? "";

        var action =
            context.RouteData.Values["action"]?.ToString() ?? "";

        var request = context.HttpContext.Request;

        bool Eslesir(
            string beklenenController,
            string beklenenAction)
        {
            return string.Equals(
                       controller,
                       beklenenController,
                       StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                       action,
                       beklenenAction,
                       StringComparison.OrdinalIgnoreCase);
        }

        var izinli =
            (Eslesir("Calisan", "Randevular")
                && HttpMethods.IsGet(request.Method))
            || (Eslesir("Account", "Cikis")
                && HttpMethods.IsPost(request.Method))
            || Eslesir("Home", "Error");

        if (izinli)
        {
            await next();
            return;
        }

        if (HttpMethods.IsGet(request.Method)
            || HttpMethods.IsHead(request.Method))
        {
            context.Result = new RedirectToActionResult(
                "Randevular",
                "Calisan",
                null);
        }
        else
        {
            context.Result = new StatusCodeResult(
                StatusCodes.Status403Forbidden);
        }
    }
}