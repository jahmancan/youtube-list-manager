using System.Data.Entity.Validation;
using System.Diagnostics;

namespace YouTubeListManager.Data.Extension
{
    internal static class DbEntityValidationExceptionExtension
    {
        internal static void ShowDebugValidationException(this DbEntityValidationException exception)
        {
            foreach (DbEntityValidationResult entityValidationResult in exception.EntityValidationErrors)
            {
                Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    entityValidationResult.Entry.Entity.GetType().Name, entityValidationResult.Entry.State);
                entityValidationResult.ShowDebugValidationErrors();
            }
        }

        private static void ShowDebugValidationErrors(this DbEntityValidationResult entityValidationResult)
        {
            foreach (DbValidationError validationError in entityValidationResult.ValidationErrors)
            {
                Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"", validationError.PropertyName,
                    validationError.ErrorMessage);
            }
        }
    }
}