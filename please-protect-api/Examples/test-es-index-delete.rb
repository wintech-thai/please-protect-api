#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
indexName = "censor-events-20260224-logstash-dispatcher-es-1"

apiUrl = "api/Es/org/#{orgId}/action/DeleteIndex/#{indexName}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
