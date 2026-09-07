using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace juggle_joy
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            name: "register",
            url: "register/{id}",
            defaults: new { controller = "Home", action = "Register", id = UrlParameter.Optional }
        );

            routes.MapRoute(
               name: "login",
               url: "login/{id}",
               defaults: new { controller = "Home", action = "login", id = UrlParameter.Optional }
           );

            routes.MapRoute(
               name: "forgot-password",
               url: "forgot-password/{id}",
               defaults: new { controller = "Home", action = "forgot_password", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                 name: "reset-password",
                 url: "reset-password/{id}",
                 defaults: new { controller = "Home", action = "ResetPassword", id = UrlParameter.Optional }
             );

          
            routes.MapRoute(
                  name: "logout",
                  url: "logout/{id}",
                  defaults: new { controller = "Home", action = "logout", id = UrlParameter.Optional }
              );

            routes.MapRoute(
                name: "our-services",
                url: "our-services/{id}",
                defaults: new { controller = "Home", action = "our_services", id = UrlParameter.Optional }
            );

             routes.MapRoute(
                name: "pricing",
                url: "pricing/{id}",
                defaults: new { controller = "Home", action = "pricing", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "our_story",
                url: "our-story/{id}",
                defaults: new { controller = "Home", action = "our_story", id = UrlParameter.Optional }
);

             routes.MapRoute(
                    name: "Faq",
                    url: "faq/{id}",
                    defaults: new { controller = "Home", action = "Faq", id = UrlParameter.Optional }
                    );


            routes.MapRoute(
          name: "registration_status_msg",
          url: "registration-status-msg/{id}",
          defaults: new { controller = "Home", action = "registration_status_msg", id = UrlParameter.Optional }
          );

            routes.MapRoute(
             name: "press and media",
             url: "press/{id}",
             defaults: new { controller = "Home", action = "press", id = UrlParameter.Optional }
             );

            routes.MapRoute(
                name: "gift_subscription",
                url: "gift-subscription/{id}",
                defaults: new { controller = "Home", action = "gift_subscription", id = UrlParameter.Optional }
                );
            routes.MapRoute(
              name: "Terms_of_Service",
              url: "Terms-of-Use/{id}",
              defaults: new { controller = "Home", action = "Terms_of_Service", id = UrlParameter.Optional }
              );

            routes.MapRoute(
              name: "Privacy_Policy",
              url: "Privacy-Policy/{id}",
              defaults: new { controller = "Home", action = "Privacy_Policy", id = UrlParameter.Optional }
              );

            routes.MapRoute(
             name: "Cookie_Policy",
             url: "Cookie-Policy/{id}",
             defaults: new { controller = "Home", action = "Cookie_Policy", id = UrlParameter.Optional }
             );

            routes.MapRoute(
             name: "Your_Privacy_Choices",
             url: "Privacy-Choices/{id}",
             defaults: new { controller = "Home", action = "Your_Privacy_Choices", id = UrlParameter.Optional }
             );

            routes.MapRoute(
            name: "Disclamer",
            url: "Disclamer/{id}",
            defaults: new { controller = "Home", action = "Disclamer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "Acceptable Use Policy",
            url: "Acceptable-Use-Policy/{id}",
            defaults: new { controller = "Home", action = "Acceptable_Use_Policy", id = UrlParameter.Optional }
            );

            routes.MapRoute(
           name: "subscription",
           url: "subscription/{id}",
           defaults: new { controller = "Home", action = "Subscription", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "RegVerification_succs_message",
                url: "RegVerification-succs-message/{id}",
                defaults: new { controller = "Home", action = "RegVerification_succs_message", id = UrlParameter.Optional }
                );

            routes.MapRoute(
               name: "Before_cancelSubscription",
               url: "Before-cancelSubscription/{id}",
               defaults: new { controller = "Home", action = "Subcancel", id = UrlParameter.Optional }
               );

            routes.MapRoute(
               name: "Confirm_cancelSubscription",
               url: "Confirm-cancelSubscription/{id}",
               defaults: new { controller = "Home", action = "beforcancel", id = UrlParameter.Optional }
               );

            routes.MapRoute(
               name: "blogs",
               url: "blogs",
               defaults: new { controller = "Blog", action = "Blogs" }
               );

            routes.MapRoute(
                  name: "blog details",
                  url: "blog-details/{blogtitle}/{id}",
                  defaults: new { controller = "Blog", action = "BlogDetail", id = UrlParameter.Optional, blogtitle = UrlParameter.Optional }
                  );

            routes.MapRoute(
                    name: "blog category details",
                    url: "blog-category/{categoryname}/{id}",
                    defaults: new { controller = "Blog", action = "blogscategorylist", id = UrlParameter.Optional, categoryname = UrlParameter.Optional }
                    );

         


            routes.MapRoute(
                  name: "Assistant-login",
                  url: "assistant-login/{id}",
                  defaults: new { controller = "Assistants", action = "assistant_login", id = UrlParameter.Optional }
                  );

            routes.MapRoute(
               name: "Assistant-forgotpassword",
               url: "assistant-forgot-password/{id}",
               defaults: new { controller = "Assistants", action = "forgot_password", id = UrlParameter.Optional }
               );

            routes.MapRoute(
                 name: "Assistant-resetpassword",
                 url: "assistant-reset-password/{id}",
                 defaults: new { controller = "Assistants", action = "ResetPassword", id = UrlParameter.Optional }
                 );


            routes.MapRoute(
                   name: "Assistant-dashboard",
                   url: "assistant-dashboard/{id}",
                   defaults: new { controller = "Assistants", action = "assistant_dashboard", id = UrlParameter.Optional }
                   );

            routes.MapRoute(
                 name: "Assistant-profile",
                 url: "assistant-profile/{id}",
                 defaults: new { controller = "Assistants", action = "assistant_profile", id = UrlParameter.Optional }
                 );


            routes.MapRoute(
                    name: "Assistant-changepassword",
                    url: "assistant-change-password/{id}",
                    defaults: new { controller = "Assistants", action = "assistant_changepassword", id = UrlParameter.Optional }
                    );

            routes.MapRoute(
                  name: "new-tasks",
                  url: "new-tasks/{id}",
                  defaults: new { controller = "Assistants", action = "new_tasks", id = UrlParameter.Optional }
                  );


            routes.MapRoute(
                  name: "pending-tasks",
                  url: "pending-tasks/{id}",
                  defaults: new { controller = "Assistants", action = "pending_tasks", id = UrlParameter.Optional }
                  );

           routes.MapRoute(
                  name: "ongoing-tasks",
                  url: "ongoing-tasks/{id}",
                  defaults: new { controller = "Assistants", action = "ongoing_tasks", id = UrlParameter.Optional }
                  );

            routes.MapRoute(
                  name: "completed-tasks",
                  url: "completed-tasks/{id}",
                  defaults: new { controller = "Assistants", action = "completed_tasks", id = UrlParameter.Optional }
                  );

            routes.MapRoute(
                name: "assistant_specific_task",
                url: "assistant-specific-task/{id}",
                defaults: new { controller = "Assistants", action = "assistant_specific_task", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                 name: "assistant_info",
                 url: "assistant-info/{id}",
                 defaults: new { controller = "Handlers", action = "assistant_info", id = UrlParameter.Optional }
                 );

            routes.MapRoute(
               name: "assistant_specific_tasks",
               url: "assistant-specific-tasks/{id}",
               defaults: new { controller = "Handlers", action = "assistant_specific_task", id = UrlParameter.Optional }
               );

            routes.MapRoute(
                name: "contact-us",
                url: "contact-us/{id}",
                defaults: new { controller = "Home", action = "contact_us", id = UrlParameter.Optional }
                );



            routes.MapRoute(
                    name: "Handler-login",
                    url: "handler-login/{id}",
                    defaults: new { controller = "Handlers", action = "handler_login", id = UrlParameter.Optional }
                    );

            routes.MapRoute(
                   name: "Handler-forgotpassword",
                   url: "handler-forgot-password/{id}",
                   defaults: new { controller = "Handlers", action = "forgot_password", id = UrlParameter.Optional }
                   );

            routes.MapRoute(
                 name: "Handler-resetpassword",
                 url: "handler-reset-password/{id}",
                 defaults: new { controller = "Handlers", action = "ResetPassword", id = UrlParameter.Optional }
                 );

            routes.MapRoute(
                name: "Handler-dashboard",
                url: "handler-dashboard/{id}",
                defaults: new { controller = "Handlers", action = "handler_dashboard", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                  name: "Handler-profile",
                  url: "handler-profile/{id}",
                  defaults: new { controller = "Handlers", action = "handler_profile", id = UrlParameter.Optional }
                  );

            routes.MapRoute(
                  name: "add-assistants",
                  url: "add-assistants/{id}",
                  defaults: new { controller = "Handlers", action = "add_assistants", id = UrlParameter.Optional }
                  );

            routes.MapRoute(
                name: "handler-changepassword",
                url: "handler-change-password/{id}",
                defaults: new { controller = "Handlers", action = "handler_changepassword", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "handler-mngassistant",
                url: "manage-assistant/{id}",
                defaults: new { controller = "Handlers", action = "manage_assistant", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                 name: "task-pending",
                 url: "task-pending/{id}",
                 defaults: new { controller = "Handlers", action = "task_pending", id = UrlParameter.Optional }
                 );

            routes.MapRoute(
            name: "task-ongoing",
            url: "task-ongoing/{id}",
            defaults: new { controller = "Handlers", action = "task_ongoing", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "task-completed",
            url: "task-completed/{id}",
            defaults: new { controller = "Handlers", action = "task_completed", id = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "user-changepassword",
                url: "change-password/{id}",
                defaults: new { controller = "Home", action = "changepassword", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "my-profile",
                url: "my-profile/{id}",
                defaults: new { controller = "Home", action = "profile", id = UrlParameter.Optional }
                );

            //Task controller
            routes.MapRoute(
                    name: "dashboard new",
                    url: "dashboard/{id}",
                    defaults: new { controller = "Task", action = "dashboard", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                  name: "pending task",
                  url: "pending/{id}",
                  defaults: new { controller = "Task", action = "pending", id = UrlParameter.Optional }
              );

            routes.MapRoute(
                 name: "ongoing task",
                 url: "ongoing/{id}",
                 defaults: new { controller = "Task", action = "ongoing", id = UrlParameter.Optional }
             );

            routes.MapRoute(
               name: "completed task",
               url: "completed/{id}",
               defaults: new { controller = "Task", action = "completed", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                 name: "add task",
                 url: "addtask/{id}",
                 defaults: new { controller = "Task", action = "addtask", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "task Details",
                url: "taskDetails/{id}",
                defaults: new { controller = "Task", action = "taskDetails", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                 name: "add minitask",
                 url: "addminitask/{id}/{minitaskid}",
                 defaults: new { controller = "Task", action = "addminitask", id = UrlParameter.Optional, minitaskid = UrlParameter.Optional }
                 );

            routes.MapRoute(
                 name: "calendly",
                 url: "book-a-call",
                 defaults: new { controller = "Home", action = "book_a_call" }
                 );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
