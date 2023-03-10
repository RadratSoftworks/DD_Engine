mergeInto(LibraryManager.library, {
    isOnMobile: function()
	{
	    return Module.SystemInfo.mobile;
	}
})