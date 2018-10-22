
activityData = null
defaultGraphOptions = {
  responsive: true,
  // scrollZoom: true, // lets us scroll to zoom in and out - works
  showLink: false, // removes the link to edit on plotly - works
  // modeBarButtonsToRemove: ['toImage', 'zoom2d', 'pan', 'pan2d', 'autoScale2d']
}
defaultLayout = {
  // bgcolor: '#111',
  // paper_bgcolor: '#111',
  // plot_bgcolor: '#111'
}

// https://stackoverflow.com/a/14991797
function parseCSV(str) {
  var arr = [];
  var quote = false;
  for (var row = col = c = 0; c < str.length; c++) {
    var cc = str[c], nc = str[c + 1];
    arr[row] = arr[row] || [];
    arr[row][col] = arr[row][col] || '';

    if (cc == '"' && quote && nc == '"') { arr[row][col] += cc; ++c; continue; }
    if (cc == '"') { quote = !quote; continue; }
    if (cc == ',' && !quote) { ++col; continue; }
    if (cc == '\n' && !quote) { ++row; col = 0; continue; }

    arr[row][col] += cc;
  }
  return arr;
}

function onActivityUploaded(files) {
  var file = files[0];
  var reader = new FileReader();
  reader.readAsText(file)
  reader.onload = () => {
    activityData = parseCSV(reader.result)
    graphActivity()
  }
}

function convertTimestamp(uts) {
  var now = new Date(uts);
  return now;
}

function graphActivity() {
  // graph activity
  var element = document.getElementById('timeline_bars');

  var data = []

  var x = []
  var y = []
  var c = []
  for (var i = 1; i < activityData.length; i++) {
    var row = activityData[i];
    var application = row[1]
    var title = row[2]
    var start = parseInt(row[6])
    var duration = parseInt(row[7])
    var startTs = convertTimestamp(start)
    var endTs = convertTimestamp(start + duration)
    
    x.push(startTs)
    y.push(duration / 1000)
    c.push(`${application} - ${title}`)
  }
  data.push({
    x: x,
    y: y,
    type: 'scatter',
    opacity: 0.5,
    line: { width: 20 },
    mode: 'markers',
    text: c,
    name: 'usage'
  })

  var layout = {
    yaxis: {
      // showgrid: false,
      // zeroline: false,
      // showline: false,
      // showticklabels: false
    }
  }
  Plotly.newPlot(element, data, { ...defaultLayout, ...layout }, { ...defaultGraphOptions });
}
