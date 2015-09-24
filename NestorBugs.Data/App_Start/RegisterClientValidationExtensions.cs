using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Konamiman.NestorBugs.Data.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace Konamiman.NestorBugs.Data.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            //DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}