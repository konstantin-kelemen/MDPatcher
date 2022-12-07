using System;
using System.IO;

public class RawImageStream : FileStream
{
	int mode = 0;
	long currentPos = 0;
	
	public RawImageStream(string path, FileMode mode) : base(path, mode)
	{
		
	}
	
	public RawImageStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
	{
		
	}
	
	public override long Seek(
		long offset,
		SeekOrigin origin)
	{
		if(mode == 0)
			return base.Seek(offset, origin);
		
		switch(origin)
		{
			case SeekOrigin.Begin:
				currentPos = offset;
			break;
			
			case SeekOrigin.End:
				currentPos = Length;
			break;
			
			case SeekOrigin.Current:
				currentPos += offset;
			break;
		}
		
		if(currentPos > Length)
			currentPos = Length;
		
		return currentPos;
	}
	
	public override int Read(
	byte[] array,
	int offset,
	int count
	)
	{		
		int c=0;
		
		if(mode == 0)
			return base.Read(array, offset, count);
		
		while(count > 0)
		{
			base.Seek( ( (currentPos/2048) * 2352 ) + ((mode==1)?16:32) + (currentPos % 2048), SeekOrigin.Begin);
			
			int r = 2048 - ((int)currentPos % 2048);
			
			if(count < r)
				r = count;
			
			c += base.Read(array, offset, r);
			
			offset+=r;
			count-=r;
			currentPos+=r;
		}
		
		return c;
	}
	
	public override void Write(
	byte[] array,
	int offset,
	int count
	)
	{
		if(mode == 0)
		{
			base.Write(array, offset, count);
			return;
		}
			
		while(count > 0)
		{
			base.Seek( ( (currentPos/2048) * 2352 ) + ((mode==1)?16:32) + (currentPos % 2048), SeekOrigin.Begin);
			
			int r = 2048 - ((int)currentPos % 2048);
			
			if(count < r)
				r = count;
			
			base.Write(array, offset, r);
			
			offset+=r;
			count-=r;
			currentPos+=r;
		}
	}
		
	public void SetMode(int mode)
	{
		this.mode = mode;
	}
}
