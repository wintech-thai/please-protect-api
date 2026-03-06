#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
groupId = 'for-dispatcher-es'

apiUrl = "api/KafkaAdmin/org/#{orgId}/action/GetConsumerGroupLag/#{groupId}"
param = {}

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
