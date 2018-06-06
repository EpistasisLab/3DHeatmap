#!/usr/bin/perl -w

if(@ARGV != 2)
{
	print "usage: ./merge_two.pl ID data.txt\n";
	exit(1);
}

$ID    = $ARGV[0];
open F1, $ARGV[1] or die;
open ROWS, ">heatrows_$ID.txt" or die;

$rnum = 1;
<F1>;
while(<F1>)
{
	chomp;
	@row = split(/\t/);
	print ROWS ($rnum."\t".shift(@row)."\n");
	$rnum++;
}
close F1;
close ROWS;

