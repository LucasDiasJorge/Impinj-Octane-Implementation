namespace ImpinjWriteTags
{
    public class TagEncode
    {
        public string CurrentEpc { get; private set; }  
        public string Tid { get; private set; }        
        public string NewEpc { get; private set; }     
        public bool Encoded { get; set; }               

        public TagEncode(string currentEpc, string tid, string newEpc)
        {
            CurrentEpc = currentEpc;
            Tid = tid;
            NewEpc = newEpc;
        }
    }
}