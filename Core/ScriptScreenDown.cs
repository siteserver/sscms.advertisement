namespace SSCMS.Advertisement.Core
{
    public class ScriptScreenDown
    {
        private readonly Models.Advertisement _advertisement;

        public ScriptScreenDown(Models.Advertisement advertisement)
        {
            _advertisement = advertisement;
        }

        public string GetScript()
        {
            var sizeString = _advertisement.Width > 0
                ? $"width={_advertisement.Width} "
                : string.Empty;
            sizeString += _advertisement.Height > 0 ? $"height={_advertisement.Height}" : string.Empty;

            return $@"
<script language=""javascript"" type=""text/javascript"">
function ad_changediv(){{
    jQuery('#ad_hiddenLayer_{_advertisement.Id}').slideDown();
    setTimeout(""ad_hidediv()"",{_advertisement.Delay}000);
}}
function ad_hidediv(){{
    jQuery('#ad_hiddenLayer_{_advertisement.Id}').slideUp();
}}
jQuery(document).ready(function(){{
    jQuery('body').prepend('<div id=""ad_hiddenLayer_{_advertisement.Id}"" style=""display: none;""><center><a href=""{_advertisement.NavigationUrl}"" target=""_blank""><img src=""{_advertisement.ImageUrl}"" {sizeString} border=""0"" /></a></center></div>');
    setTimeout(""ad_changediv()"",2000);
}});
</script>
";
        }
    }
}
