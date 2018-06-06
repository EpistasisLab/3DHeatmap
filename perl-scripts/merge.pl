#!/usr/bin/perl -w
# use strict;

if(@ARGV != 3)
{
	print "usage: ./merge.pl ID height.txt marker.txt\n";
	exit(1);
}

$ID    = $ARGV[0];

open F1, $ARGV[1] or die;
open F2, $ARGV[2] or die;

$ARGV[1] =~ s/^.*[\/\\]//g;
$ARGV[2] =~ s/^.*[\/\\]//g;

$ARGV[1] =~ s/\.txt$//g;
$ARGV[2] =~ s/\.txt$//g;

open TMP, ">tmp_$ID"."_".$ARGV[1]."_".$ARGV[2].".txt" or die;

$rnum = 1;
<F1>;
while(<F1>)
{
	chomp;
	@row = split(/\t/);
	shift @row;
	$cnum = 1;
	foreach $x (@row)
	{
		$x =~ s/\s//g;
		push @{ $data[$rnum][$cnum] }, $x;
		# print $rnum."\t".$cnum."\t".$x."\n";
		$cnum++;
	}
	$rnum++;
}
close F1;

$rnum = 1;
<F2>;
while(<F2>)
{
	chomp;
	@row = split(/\t/);
	shift @row;
	$cnum = 1;
	foreach $x (@row)
	{
		$x =~ s/\s//g;
		if($rnum == 1 && $cnum == 1)
		{
			$mmax = $x;
			$mmin = $x;
		} 
		else 
		{
			if($x > $mmax)
			{
				$mmax = $x;
			}
			if($x < $mmin)
			{
				$mmin = $x;
			}
		}
		push @{ $data[$rnum][$cnum] }, $x;
		$cnum++;
	}
	$rnum++;
}
close F2;

$mlen = $mmax - $mmin;
#print "mmax ".$mmax."\n";
#print "mmin ".$mmin."\n";
#print "mlen ".$mlen."\n";

$I = scalar(@data);
$J = scalar(@{ $data[1] });

for ($i = 1;  $i < $I; $i++ )
{
	for ($j = 1; $j < $J; $j++ )
	{
		$h = ${ $data[$i][$j] }[0] ;
		$m = ${ $data[$i][$j] }[1] ;
		$m = ( $m - $mmin ) * 1000 / $mlen;
		$m = sprintf "%d", $m;
		# print TMP $i."\t".$j."\t".join("\t<bin>\t", @{ $data[$i][$j] })."\n";
		print TMP $i."\t".$j."\t".$h."\t<bin>\t".$m."\n";
	}
}

close TMP;
