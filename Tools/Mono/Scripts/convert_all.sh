#!/bin/sh
script_dir=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
extract_tool="$script_dir/../MkBundleExtract/Release/MkBundleExtract.exe"
data_dir="$script_dir/Data"
data_dir_win=$(cygpath -w "$data_dir")
echo "data: $data_dir_win"
"$extract_tool" "$data_dir_win/mandroid.exe" "mandroid"
if [ $? -ne 0 ]; then
    echo "extract failed"
    exit
fi
"$extract_tool" "$data_dir_win/mtouch.exe" "mtouch"
if [ $? -ne 0 ]; then
    echo "extract failed"
    exit
fi
mv "$data_dir/mandroid/Mono.Touch.Client.dll" "$data_dir/mandroid/Mono.Touch.Client_.dll"
mv "$data_dir/mandroid/Mono.Touch.Common.dll" "$data_dir/mandroid/Mono.Touch.Common_.dll"
mv "$data_dir/mtouch/Mono.Touch.Activation.Common.dll" "$data_dir/mtouch/Mono.Touch.Activation.Common_.dll"
cp -a "$script_dir/../Support/MonoTouchClient/bin/Release/Mono.Touch.Client.dll" "$data_dir/mandroid/"
cp -a "$script_dir/../Support/MonoTouchCommon/bin/Release/Mono.Touch.Common.dll" "$data_dir/mandroid/"
cp -a "$script_dir/../Support/MonoTouchCommon/bin/Release/Mono.Touch.Activation.Common.dll" "$data_dir/mtouch/"
pushd "$data_dir/mandroid/"
"../../create_bundle_android.sh"
popd
pushd "$data_dir/mtouch/"
"../../create_bundle_touch.sh"
popd
