namespace OPAStyraWebAPI.Permissions
{
    public class RbacRequest
    {
        public RbacPermissionRequest Input { get; set; }
    }

    public class RbacPermissionRequest
    {
        public string Role { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
    }
}
