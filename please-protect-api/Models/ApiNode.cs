namespace Its.PleaseProtect.Api.Models
{
    public class ApiNode
    {
        public string ControllerName { get; set; }
        public string ApiName { get; set; }

        public bool IsAllowed { get; set; }

        public ApiNode()
        {
            IsAllowed = false;
            ApiName = "";
            ControllerName = "";
        }
    }
}
