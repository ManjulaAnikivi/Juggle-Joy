using System.Web.Mvc;

namespace juggle_joy.Areas.admin
{
    public class adminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {


            context.MapRoute(
           "admin dashboard",
           "admin/dashboard",
           new { controller = "account", action = "dashboard", id = UrlParameter.Optional }
           );

            context.MapRoute(
           "admin change password",
           "admin/change-password",
           new { controller = "account", action = "change_password", id = UrlParameter.Optional }
           );

            context.MapRoute(
            "admin profile",
            "admin/profile",
            new { controller = "account", action = "update_profile", id = UrlParameter.Optional }
            );

            context.MapRoute(
              "admin forgot password",
              "admin/forgot-password/{id}",
              new { controller = "account", action = "forgot_password", id = UrlParameter.Optional }
              );

            context.MapRoute(
             "admin reset password",
             "admin/reset-password/{id}",
             new { controller = "account", action = "reset_password", id = UrlParameter.Optional }
             );


            //assistant controller

            context.MapRoute(
            "admin add assistant",
            "admin/add-assistant/{id}",
            new { controller = "assistant", action = "Register", id = UrlParameter.Optional }
            );

            context.MapRoute(
              "active assistants",
              "admin/active-assistants/{id}",
              new { controller = "assistant", action = "active", id = UrlParameter.Optional }
            );


            context.MapRoute(
              "blocked assistants",
              "admin/blocked-assistants/{id}",
              new { controller = "assistant", action = "blocked", id = UrlParameter.Optional }
            );

            //Blog Controller
            context.MapRoute(
             "admin add blog",
             "admin/add-blog",
             new { controller = "Blogs", action = "addblog", id = UrlParameter.Optional }
             );

            context.MapRoute(
            "admin manage blog",
            "admin/manage-blog",
            new { controller = "Blogs", action = "bloglist", id = UrlParameter.Optional }
            );


                        context.MapRoute(
            "admin add blog category",
            "admin/add-blogcategory",
            new { controller = "Blogs", action = "manageblogcategory", id = UrlParameter.Optional }
            );

            context.MapRoute(
            "admin manage blog category",
            "admin/manage-blogcategory",
            new { controller = "Blogs", action = "blogcategory_list", id = UrlParameter.Optional }
            );

            //register price

            context.MapRoute(
            "admin manage price",
            "admin/manage_price",
            new { controller = "price", action = "addPrice", id = UrlParameter.Optional }
            );

            //contact Controller
            context.MapRoute(
            "admin contacts",
            "admin/contacts",
            new { controller = "contact", action = "list" }
            );
            context.MapRoute(
           "admin subscriber",
           "admin/subscribers",
           new { controller = "contact", action = "subscriber" }
           );

            //FAQ

            context.MapRoute(
          "admin Faq_category",
          "admin/manage-Faq-category/{id}",
          new { controller = "cms", action = "Faq_category", id = UrlParameter.Optional }
          );
            context.MapRoute(
            "admin add faq",
            "admin/add-faq/{id}",
            new { controller = "cms", action = "manage_faq", id = UrlParameter.Optional }
            );

            context.MapRoute(
             "admin manage faq",
             "admin/faq-list/{id}",
             new { controller = "cms", action = "faq_list", id = UrlParameter.Optional }
             );

            context.MapRoute(
             "admin cms",
             "admin/manage-cms",
             new { controller = "cms", action = "manage", id = UrlParameter.Optional }
             );
            //Question Controller

            context.MapRoute(
            "admin Question",
            "admin/questions/{id}",
            new { controller = "question", action = "Addquestion", id = UrlParameter.Optional }
            );

            context.MapRoute(
            "admin Manage-Question",
            "admin/manage-question/{id}",
            new { controller = "question", action = "managequestion", id = UrlParameter.Optional }
            );

            context.MapRoute(
           "admin delete_que",
           "admin/delete_que/{id}",
           new { controller = "question", action = "delete_que", id = UrlParameter.Optional }
           );

            context.MapRoute(
           "admin del-question",
           "admin/delete-question/{id}",
           new { controller = "question", action = "Delete_question", id = UrlParameter.Optional }
           );


            //category controller
            context.MapRoute(
            "admin add category",
            "admin/add-category/{id}",
            new { controller = "category", action = "manage", id = UrlParameter.Optional }
            );

            //subcategory controller
            context.MapRoute(
            "admin add subcategory",
            "admin/add-subcategory/{id}",
            new { controller = "category", action = "subcategory", id = UrlParameter.Optional }
            );


            //userManagement Controller

            context.MapRoute(
           "incomplete_registration",
           "admin/incomplete-registred-users/{id}",
           new { controller = "userManagement", action = "incomplete_registration", id = UrlParameter.Optional }
           );

            context.MapRoute(
            "active-users",
            "admin/active-users/{id}",
            new { controller = "userManagement", action = "active", id = UrlParameter.Optional }
            );

            context.MapRoute(
            "blocked-users",
            "admin/blocked-users/{id}",
            new { controller = "userManagement", action = "blocked", id = UrlParameter.Optional }
            );

            //Banner Controller
            context.MapRoute(
            "admin add banner",
            "admin/add-banner/{id}",
            new { controller = "banner", action = "addbanner", id = UrlParameter.Optional }
            );

            //handler controller

            context.MapRoute(
            "admin add handler",
            "admin/add-handler/{id}",
            new { controller = "handler", action = "Register", id = UrlParameter.Optional }
            );

            context.MapRoute(
              "active handler",
              "admin/active-handlers/{id}",
              new { controller = "handler", action = "active", id = UrlParameter.Optional }
            );


            context.MapRoute(
              "blocked handler",
              "admin/blocked-handlers/{id}",
              new { controller = "handler", action = "blocked", id = UrlParameter.Optional }
            );

            context.MapRoute(
            "admin collection",
            "admin/{controller}/{action}/{id}",
            new { controller = "account", action = "login", id = UrlParameter.Optional }
            );



            //context.MapRoute(
            //    "admin_default",
            //    "admin/{controller}/{action}/{id}",
            //    new { action = "login", id = UrlParameter.Optional }
            //);
        }
    }
}