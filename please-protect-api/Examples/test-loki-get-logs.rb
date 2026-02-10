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
limit     = 100

logql = '{namespace="pp-development"}'

queryRangeStr = URI.encode_www_form(
  query: logql,
  start: start_ts,
  end: end_ts,
  limit: limit
)

queryStr = URI.encode_www_form(
  query: logql,
)

apiUrl = "api/Proxy/org/#{orgId}/action/Loki/loki/api/v1/query_range?#{queryRangeStr}"
#apiUrl = "api/Proxy/org/#{orgId}/action/Loki/loki/api/v1/query?#{queryStr}"
param = nil

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
