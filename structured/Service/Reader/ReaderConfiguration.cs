using Impinj.OctaneSdk;
using Service.Reader.Interfaces;

namespace Service.Reader;

public class ReaderConfiguration : IReaderConfiguration
{
    private ImpinjReader _impinjReader;

    public ReaderConfiguration(ImpinjReader impinjReader)
    {
        _impinjReader = impinjReader;
    }

    public Settings SetConfig()
    {
        Settings settings = _impinjReader.QueryDefaultSettings();
        settings.Report.Mode = ReportMode.Individual;
        settings.Report.IncludeFirstSeenTime = true;
        settings.Antennas.GetAntenna(1).IsEnabled = true;
        settings.Antennas.GetAntenna(1).TxPowerInDbm = 24.0;

        return settings;
    }
}