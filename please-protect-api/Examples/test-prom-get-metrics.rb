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

apiUrl = "api/Proxy/org/#{orgId}/action/Prometheus/api/v1/query?query=up"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
