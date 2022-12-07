all:
	mcs -r:DiscUtils.dll -r:DiscUtils.Common.dll -r:System.Windows.Forms.dll -r:System.Drawing.dll mdpatcher.cs \
		pattern.cs rawimagestream.cs
cli:
        mcs -r:DiscUtils.dll -r:DiscUtils.Common.dll mdpatcher-cli.cs pattern.cs rawimagestream.cs
