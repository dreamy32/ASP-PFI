using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MySpace
{
    public static class DateFormatExtension
    {
        //public enum JoursType { lundi, mardi, mercredi, jeudi, vendredi, samedi, dimanche }
        public enum MoisType { janvier, février, mars, avril, mai, juin, juillet, août, septembre, octobre, novembre, décembre }
        public static string ToLongDateStringFRCA(this DateTime date)
        {
            return date.Day.ToString() + " " + ((MoisType)(date.Month - 1)).ToString() + " " + date.Year.ToString();
        }
        public static string NumToString(int num)
        {
            return (num < 10 ? "0" + num.ToString() : num.ToString());
        }
        public static string ToShortDateStringFRCA(this DateTime date)
        {
            return NumToString(date.Day) + "-" + NumToString(date.Month) + "-" + NumToString(date.Year);
        }
        public static string ToShortTimeStringFRCA(this DateTime date)
        {
            return NumToString(date.Hour) + ":" + NumToString(date.Minute);
        }

        public static string When(this DateTime date)
        {
            TimeSpan span = DateTime.Now.Subtract(date);
            DateTime localTime = date.Subtract(new TimeSpan((int)HttpContext.Current.Session["TimeZoneOffset"], 0, 0));
            if (span.Days > 0)
            {
                return localTime.ToLongDateStringFRCA();
            }
            else
            {
                if (span.Hours > 1)
                {
                    return "Il y a " + span.Hours + " heures";
                }
                else
                {
                    if (span.Minutes > 1)
                    {
                        return "Il y a " + span.Minutes + " minutes";
                    }
                    else
                    {
                        return "À l'instant";
                    }
                }
            }
        }
    }
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
