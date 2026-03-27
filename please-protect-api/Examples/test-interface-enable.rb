#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
interfaceId = 'enp2s0'

apiUrl = "api/Interface/org/#{orgId}/action/EnableInterface/#{interfaceId}"
#apiUrl = "api/Interface/org/#{orgId}/action/DisableInterface/#{interfaceId}"
param = nil

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
