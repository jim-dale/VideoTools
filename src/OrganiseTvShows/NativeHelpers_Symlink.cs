
namespace OrganiseTvShows
{
    public static partial class NativeHelpers
    {
        public static void CreateSymbolicFileLink(string sourceFileName, string destFileName)
        {
            NativeMethods.CreateSymbolicLink(destFileName, sourceFileName, NativeMethods.SYMBOLIC_LINK_FLAG.File | NativeMethods.SYMBOLIC_LINK_FLAG.AllowUnprivilegedCreate);
        }
    }
}
