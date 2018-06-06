#!/usr/bin/perl -w

use DBI;

if(@ARGV != 1)
{
	print "usage: ./import.pl ID\n";
	exit(1);
}

$ID =  $ARGV[0];
$heat_table = "heat_$ID";
$heatrows_table = "heatrows_$ID";

$dbh = DBI->connect('DBI:SQLite:testdata.sqlite', '', '', { AutoCommit => 1});
$dbh->do("PRAGMA synchronous = OFF");

$dbh->do("DROP TABLE IF EXISTS \"$heat_table\"");
$dbh->do("DROP TABLE IF EXISTS \"$heatrows_table\"");

$dbh->do("CREATE TABLE \"$heat_table\" (\"row\" INTEGER, \"col\" INTEGER, \"height\" FLOAT, \"bin\" INTEGER, \"marker\" INTEGER)");
$dbh->do("CREATE TABLE \"$heatrows_table\" (\"row\" INTEGER, \"name\" TEXT)");


print "importing heatrows_$ID.txt to testdata.sqlite...\n";
open ROWS, "heatrows_$ID.txt" or die;
my $addstatement = "insert into $heatrows_table (row, name) values (?, ?)";
my $addexec = $dbh->prepare($addstatement);
while(<ROWS>)
{
	chomp;
	@x = split/\t/;
	$addexec->execute(( @x ));
}
$addexec->finish();
close ROWS;

print "importing heat_$ID.txt to testdata.sqlite...\n";
open HEAT, "heat_$ID.txt" or die;
$addstatement = "insert into $heat_table (row, col, height, bin, marker) values (?, ?, ?, ?, ?)";
$addexec = $dbh->prepare($addstatement);
while(<HEAT>)
{
	chomp;
	@x = split/\t/;
	$addexec->execute(( @x ));
}
$addexec->finish();
close HEAT;

$dbh->disconnect();


