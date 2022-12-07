all:
	mcs -r:DiscUtils.dll -r:DiscUtils.Common.dll -r:System.Windows.Forms.dll -r:System.Drawing.dll mdpatcher.cs \
		pattern.cs rawimagestream.cs
