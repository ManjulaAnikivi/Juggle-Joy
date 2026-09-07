using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace juggle_joy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
            //filters.Add(new RequireHttpsAttribute()); This is only for jugglejoy.com for other need to uncomment
            //we are forcing https with this
            //filters.Add(new RequireHttpsAttribute());
        }
    }
}
