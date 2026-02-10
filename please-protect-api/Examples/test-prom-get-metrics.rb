#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
keyword = ''

start_ts = (Time.now - 3600).to_i   # 1 ชั่วโมงที่แล้ว
end_ts   = Time.now.to_i
step     = 30

promql = 'node_load1{job="node-exporter"}'
promql = 'node_load5{job="node-exporter"}'
promql = 'node_load15{job="node-exporter"}'
promql = 'count(node_cpu_seconds_total{job="node-exporter", mode="idle"})' #นับจำนวน core
promql = 'count(node_cpu_seconds_total{job="node-exporter", mode="idle"})' #นับจำนวน core
promql = 'node_filesystem_size_bytes{job="node-exporter"}'
promql = 'node_filesystem_avail_bytes'

encoded = URI.encode_www_form_component(promql)

queryRangeStr = URI.encode_www_form(
  query: promql,
  start: start_ts,
  end: end_ts,
  step: step
)

queryStr = URI.encode_www_form(
  query: promql,
)

#apiUrl = "api/Proxy/org/#{orgId}/action/Prometheus/api/v1/query_range?#{queryRangeStr}"
apiUrl = "api/Proxy/org/#{orgId}/action/Prometheus/api/v1/query?#{queryStr}"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
