using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Konamiman.NestorBugs.Web.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace Konamiman.NestorBugs.Web.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            //DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}