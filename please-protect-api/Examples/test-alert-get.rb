#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/AlertEvent/org/#{orgId}/action/GetAlertEvents"
param = {
  FullTextSearch: "",
  FromDate: "2026-03-11T00:00:00Z",
  ToDate: "2026-03-11T23:59:59Z",
}

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)

apiUrl = "api/AlertEvent/org/#{orgId}/action/GetAlertEventCount"
result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)