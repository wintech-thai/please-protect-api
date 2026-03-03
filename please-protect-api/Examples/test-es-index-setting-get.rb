#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
index = "censor-events-20260303-logstash-dispatcher-es-1"

apiUrl = "api/Es/org/#{orgId}/action/GetIndexSetting/#{index}"
param = nil
result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
