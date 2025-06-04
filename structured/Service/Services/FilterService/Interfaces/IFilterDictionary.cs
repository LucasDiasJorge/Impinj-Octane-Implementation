using Impinj.OctaneSdk;

namespace Service.Services.FilterService.Interfaces;

public interface IFilterDictionary
{ 
    public bool ShouldReportTag(string epc);

}