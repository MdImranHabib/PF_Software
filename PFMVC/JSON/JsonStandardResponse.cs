using System.Collections.Generic;
using System.Linq;

namespace CustomJsonResponse
{
    /// <summary>
    /// Note: I am using dynamic objects mainly for efficiency. 
    ///        This way I only send back to he client what I really need.
    /// </summary>
    public class JsonResponseFactory
    {
        public static object ErrorResponse(List<string> errorList) {

            string error = errorList.Aggregate(string.Empty, (current, item) => current + ("<li>" + item + "</li>"));

            return new { Success = false, ErrorMessage = error };
        }

        public static object SuccessResponse() {
            return new { Success = true};
        }


    }
}