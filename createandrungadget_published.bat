@echo off
pushd
cd BitcoinWPFGadget/publish
del *.html *.gadget *.xml *.png
cp ../gadget.html .
cp ../gadget.xml .
cp ../BC_Logo_.png .
7z a -r BTCGuildStats.zip *>nul
rename *.zip *.gadget
BTCGuildStats.gadget
popd
