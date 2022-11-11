set -x
rm -rf temp/
mkdir temp/

destination=temp/
runtime=$destination/Runtime
mkdir $runtime

cp Runtime.meta $destination

cp ../src/lib/package.json* $destination

cp -r ../src/lib/Surge $runtime
cp ../src/lib/Surge.meta $runtime
cp com.outbreakstudios.surge.base.asmdef* $runtime

cd $destination

npm pack
