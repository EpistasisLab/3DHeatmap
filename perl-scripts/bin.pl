#!/usr/bin/perl -w
use Cwd;
use DirHandle;

# http://www.daniweb.com/forums/thread131212.html

if(@ARGV != 1)
{
	print "usage: ./bin.pl ID\n";
	exit(1);
}

$ID =  $ARGV[0];
$wd =  getcwd();
$bin = 1;

my $dh = DirHandle->new($wd);
open OUT, ">heat_$ID.txt" or die;

# open output file 

while(defined(my $file = $dh->read))
{
	if( $file =~ /^tmp_$ID/ )
	{
		print $bin."\t".$file."\n";
		open IN, $file or die;
		while(<IN>)
		{
			s/<bin>/$bin/g;
			print OUT;
		}
		close IN; 
		$bin++;
	}
}
close OUT;
$dh->close();